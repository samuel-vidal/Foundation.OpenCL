

// shared memory has 16 lanes of 32 bits each

inline half2 coalesced_load_32(half * p, int tid)
{
	int lane = ((int *) p)[tid];
	int low = sub_group_shuffle(lane, tid / 2);
	int high = sub_group_shuffle(lane, 8 + tid / 2);
	int h = tid % 2;
	return (half2) (as_half2(low)[h], as_half2(high)[h]); 
}

inline half coalesced_load_16(__global half * p, int tid)
{
	int lane = 0;
	
	if (tid < 8) lane = ((int *) p)[tid];
	int low = sub_group_shuffle(lane, tid / 2);
	int h = tid % 2;
	return as_half2(low)[h]; 
}

inline void coalesced_store_16(__global half * p, half v, int tid)
{
	int i = (2 * tid) % 16;
	half t0 = sub_group_shuffle(v, i);
	half t1 = sub_group_shuffle(v, i + 1);
	int lane = as_int((half2)(t0, t1));

	if (tid < 8) ((int *) p)[tid] = lane;
}

__attribute__((intel_reqd_sub_group_size(16)))
__kernel void gemm_b_m_n(
    __global half* c,
    __global const half* a,
    __global const half* b,
	int batchSize,
	int inputDim,
	int outputDim)
{
	const int max_batch_size = 32;
	const int max_num_block = 32;

	__local int16 a_tile [2][max_batch_size];
	__local int16 b_tile [2][16 * max_num_block];

	__private int8 b_reg;

	__private half2 a_reg;
	__private float c_reg;

    const int tid = get_sub_group_local_id() % 16;
	// const int j = get_sub_group_id ();

	const int batch_index = get_local_id(1);
	const int num_block = get_local_size(2);
	const int first_block = num_block * get_global_id(2);
	const int local_block = get_local_id(2);
	const int out_block = local_block + first_block;

	c_reg = coalesced_load_16(c + outputDim * batch_index, tid);

	int current = 1;
	int next = 0;
	
    event_t completion_a = async_work_group_strided_copy(
		&a_tile[next][0], (__global int16 *)a, batchSize, inputDim / 32, 0);

    event_t completion_b = async_work_group_strided_copy(
		&b_tile[next][0], (__global int16 *)(b + inputDim * first_block), 16 * num_block, inputDim / 32, 0);

	const num_iterations = k_a_stride / 32;

	for(int k = 0; k < num_iterations ; k++)
	{
		current = 1 - current;
		next = 1 - next;
		 
		wait_group_events (1, &completion_a);			// wait for current a
		
		if (k + 1 < num_iterations)
		{
			completion_a = async_work_group_strided_copy(
				&a_tile[next][0], (__global int16 *)(a + 32 * k + 32), batchSize , inputDim / 32, 0);
		}

		a_reg = coalesced_load_32(&a_tile[current][batch_index], tid);

		wait_group_events (1, &completion_b);			// wait for current b
		
		if (k + 1 < num_iterations)
		{
			completion_b = async_work_group_strided_copy(
				&b_tile[next][0], (__global int16 *)(b + 32 * k + 32), 16 * num_block, inputDim / 32, 0);
		}

		b_reg = vload8(tid * 2, (int*) &b_tile[current][16 * local_block]);					// East
	
		c_reg = intel_sub_group_f16_f16_matrix_mad_k16(as_short(a_reg[0]), b_reg, c_reg);
	
		b_reg = vload8(tid * 2 + 1, (int*) &b_tile[current][16 * local_block]);				// West

		c_reg = intel_sub_group_f16_f16_matrix_mad_k16(as_short(a_reg[1]), b_reg, c_reg);
	}

	coalesced_store_16(c + outputDim * batch_index, c_reg, tid);
}