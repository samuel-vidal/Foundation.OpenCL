


// shared memory has 16 lanes of 32 bits each

inline half2 coalesced_load_32(local half * p, int tid)
{
    int lane = ((int *) p)[tid];
    int low = sub_group_shuffle(lane, tid / 2);
    int high = sub_group_shuffle(lane, 8 + tid / 2);
    int h = tid % 2;
    return (half2) (as_half2(low)[h], as_half2(high)[h]); 
}

inline half coalesced_load_16(half * p, int tid)
{
    int lane = 0;
	
    if (tid < 8) lane = ((int *) p)[tid];
    int low = sub_group_shuffle(lane, tid / 2);
    int h = tid % 2;
    return as_half2(low)[h]; 
}

inline void coalesced_store_16(half * p, half v, int tid)
{
    int i = (2 * tid) % 16;
    half t0 = sub_group_shuffle(v, i);
    half t1 = sub_group_shuffle(v, i + 1);
    int lane = as_int((half2)(t0, t1));

    if (tid < 8) ((int *) p)[tid] = lane;
}

enum
{
    k_subgroup_count = 16,
    k_sub_group_size = 16,
	k_iterations = 2048 / (2 * k_sub_group_size * k_subgroup_count),		// 4
    k_stride = 2048
};

__attribute__((intel_reqd_sub_group_size(16)))
kernel void gemm_1_2048_16n_fused_layer_norm(
    global const half* c,
    global const half* a,
    global const half* b)
{
    const int tid = get_sub_group_local_id();
    const int sid = get_sub_group_id();
    const int j = get_group_id(0);

	local float reduce[k_subgroup_count][k_sub_group_size];			// 1KB

    local int16 a_tile [2][k_subgroup_count];						// 2KB
    local int16 b_tile [2][k_sub_group_size][k_subgroup_count];		// 32KB

    private int16 b_reg;

    private half a_reg0;
    private half a_reg1;
    private float c_reg = 0.0f;
	private float sq_norm = 0.0f;

    int current = 1;
    int next = 0;

    event_t completion_b [16];
    for (int i = 0; i < 16; i++)
    {
        completion_b[i] = async_work_group_copy(&b_tile[next][i][0], (global int16 *)(b + (j * 16 + i) * k_stride), k_subgroup_count, 0);
    }

    event_t completion_a = async_work_group_copy(&a_tile[next][0], (global int16 *)a, k_subgroup_count, 0);

    for (int k = 0; k < k_iterations; k++)
    {
        current = 1 - current;
        next = 1 - next;

        wait_group_events (16, &completion_b[0]);		// wait for current b
		
        if (k + 1 < k_iterations)
        {
            for (int i = 0; i < 16; i++)
            {
                completion_b[i] = async_work_group_copy(&b_tile[next][i][0], (global int16 *)(b + (j * 16 + i) * k_stride + (k+1) * 32 * k_subgroup_count), k_subgroup_count, 0);
            }
        }
		
        wait_group_events (1, &completion_a);			// wait for current a
		
        if (k + 1 < k_iterations)
        {
            completion_a = async_work_group_copy(&a_tile[next][0], (global int16 *)(a + (k + 1) * 32 * k_subgroup_count), k_subgroup_count, 0);
        }

        half2 tmp = coalesced_load_32((local half *)&a_tile[current][sid], tid);
        a_reg0 = tmp[0];
        a_reg1 = tmp[1];

		sq_norm += a_reg0 * a_reg0 + a_reg1 * a_reg1;
		
		b_reg = vload16(tid * k_subgroup_count, (int*) &b_tile[current][0][sid]);

        c_reg = intel_sub_group_f16_f16_matrix_mad_k16(as_short(a_reg0), b_reg.lo, c_reg);

        c_reg = intel_sub_group_f16_f16_matrix_mad_k16(as_short(a_reg1), b_reg.hi, c_reg);
    }

    // Epilogue

	float scale = 1.0f / sqrt(work_group_reduce_add(sq_norm) / k_stride + 1e-6f);

    for (int s = k_subgroup_count; s > 0; s >>= 1)
    {
        if (sid >= s && sid < 2 *s) reduce[sid][tid] = c_reg;

        work_group_barrier(CLK_LOCAL_MEM_FENCE);

        if (sid < s) c_reg += reduce[sid + s][tid];
    }

    if (sid == 0)
    {
        coalesced_store_16(c + j * 16, c_reg * scale, tid);
    }
}