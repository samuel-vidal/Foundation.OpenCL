

// shared memory has 16 lanes of 32 bits each

inline half2 coalesced_load_32(__global half * p, int tid)
{
	int lane = ((int *) p)[tid];
	int low = sub_group_shuffle(lane, tid / 2);
	int high = sub_group_shuffle(lane, 8 + tid / 2);
	int h = tid % 2;
	return (half2) (as_half2(low)[h], as_half2(high)[h]); 
}

// shared memory has 16 lanes of 32 bits each

inline void coalesced_store_32(__global half * p, half2 v, int tid)
{
	int i = (2* tid ) % 16;
	half2 t0 = as_half2(sub_group_shuffle(as_int(v), i));
	half2 t1 = as_half2(sub_group_shuffle(as_int(v), i + 1));
	int lane = tid < 8 ? 
		as_int((half2)(t0[0], t1[0])) :
		as_int((half2)(t0[1], t1[1]));
	((int *) p)[tid] = lane;
}

enum
{
	k_b_stride = 32,
	k_a_stride = 32,
	k_c_stride = 32
};

__attribute__((intel_reqd_sub_group_size(16)))
__kernel void gemm_8_32_32(
    __global half* c,
    __global const half* a,
    __global const half* b)
{
	__local int16 b_tile [32];

	__private int8 b_reg;

	__private half8 a_reg0;
	__private half8 a_reg1;
	__private float8 c_reg0;
	__private float8 c_reg1;

    const int tid = get_sub_group_local_id() % 16;

    event_t completion[2];

    completion[0] = async_work_group_strided_copy(&b_tile[0], (__global int16 *)b, 16, k_b_stride/ 32, 0);							// top 16 rows
    completion[1] = async_work_group_strided_copy(&b_tile[16], (__global int16 *)(b + 16 * k_b_stride), 16, k_b_stride/ 32, 0);		// bottom 16 rows

	__attribute__((opencl_unroll_hint))
	for(int i = 0; i < 8; i ++)
	{
		half2 tmp = coalesced_load_32(a + k_a_stride * i, tid);
		a_reg0[i] = tmp[0];
		a_reg1[i] = tmp[1];
		
		tmp = coalesced_load_32(c + k_c_stride * i, tid);
		c_reg0[i] = tmp[0];
		c_reg1[i] = tmp[1];
	}

	wait_group_events (1, &completion[0]);

	b_reg = vload8(tid * k_b_stride / 16, (int*) &b_tile);					// NE
	
	c_reg0 = intel_sub_group_f16_f16_matrix_mad_k16(as_short8(a_reg0), b_reg, c_reg0);
	
	b_reg = vload8(tid * k_b_stride / 16 + 1, (int*) &b_tile);				// NW

	c_reg0 = intel_sub_group_f16_f16_matrix_mad_k16(as_short8(a_reg1), b_reg, c_reg0);
	
	wait_group_events (1, &completion[1]);

	b_reg = vload8(k_b_stride + tid * k_b_stride / 16, (int*) &b_tile);			// SE
	
	c_reg1 = intel_sub_group_f16_f16_matrix_mad_k16(as_short8(a_reg0), b_reg, c_reg1);
	
	b_reg = vload8(k_b_stride + tid * k_b_stride / 16 + 1, (int*) &b_tile);		// SW

	c_reg1 = intel_sub_group_f16_f16_matrix_mad_k16(as_short8(a_reg1), b_reg, c_reg1);

	__attribute__((opencl_unroll_hint))
	for(int i = 0; i < 8; i ++)
	{
		half2 tmp = (half2)((half) c_reg0[i], (half) c_reg1[i]);
		coalesced_store_32(c + k_c_stride * i, tmp, tid);
	}
}