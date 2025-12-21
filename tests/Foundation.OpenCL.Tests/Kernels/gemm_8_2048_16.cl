

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

enum
{
	k_b_stride = 2048,
	k_a_stride = 2048,
	k_c_stride = 16
};

__attribute__((intel_reqd_sub_group_size(16)))
__kernel void gemm_8_2048_16(
    __global half* c,
    __global const half* a,
    __global const half* b)
{
	__local int16 a_tile [2][8];
	__local int16 b_tile [2][16];

	__private int8 b_reg;

	__private half8 a_reg0;
	__private half8 a_reg1;
	__private float8 c_reg;

    const int tid = get_sub_group_local_id() % 16;
	// const int j = get_sub_group_id ();

	__attribute__((opencl_unroll_hint))
	for(int i = 0; i < 8; i ++)
	{
		c_reg[i] = coalesced_load_16(c + k_c_stride * i, tid);
	}

	int current = 1;
	int next = 0;
	
    event_t completion_a = async_work_group_strided_copy(&a_tile[next][0], (__global int16 *)a, 8, k_a_stride / 32, 0);
    event_t completion_b = async_work_group_strided_copy(&b_tile[next][0], (__global int16 *)b, 16, k_b_stride / 32, 0);

	const num_iterations = k_a_stride / 32;

	for(int k = 0; k < num_iterations ; k++)
	{
		current = 1 - current;
		next = 1 - next;
		 
		wait_group_events (1, &completion_a);			// wait for current a
		
		if (k + 1 < num_iterations)
		{
			completion_a = async_work_group_strided_copy(&a_tile[next][0], (__global int16 *)(a + 32 * k + 32), 8, k_a_stride / 32, 0);
		}

		__attribute__((opencl_unroll_hint))
		for(int i = 0; i < 8; i ++)
		{
			half2 tmp = coalesced_load_32(&a_tile[current][i], tid);
			a_reg0[i] = tmp[0];
			a_reg1[i] = tmp[1];
		}

		wait_group_events (1, &completion_b);			// wait for current b
		
		if (k + 1 < num_iterations)
		{
			completion_b = async_work_group_strided_copy(&b_tile[next][0], (__global int16 *)(b + 32 * k + 32), 16, k_b_stride / 32, 0);
		}

		b_reg = vload8(tid * 2, (int*) &b_tile[current][0]);					// East
	
		c_reg = intel_sub_group_f16_f16_matrix_mad_k16(as_short8(a_reg0), b_reg, c_reg);
	
		b_reg = vload8(tid * 2 + 1, (int*) &b_tile[current][0]);				// West

		c_reg = intel_sub_group_f16_f16_matrix_mad_k16(as_short8(a_reg1), b_reg, c_reg);
	}

	__attribute__((opencl_unroll_hint))
	for(int i = 0; i < 8; i ++)
	{
		coalesced_store_16(c + k_c_stride * i, c_reg[i], tid);
	}
}