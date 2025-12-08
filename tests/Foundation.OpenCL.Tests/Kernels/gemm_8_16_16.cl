



__attribute__((intel_reqd_sub_group_size(16)))
__kernel void gemm_8_16_16(
    __global half* c,
    __global const half* a,
    __global const half* b)
{
	
	// just to reinforce the register hypothesis

	__private half8 a_reg;
	__private int8 b_reg;
	__private half8 c_reg;

    const int tid = get_sub_group_local_id() % 16;

	for(int i = 0; i < 8; i ++)
	{
		c_reg[i] = c[i * 16 + tid];			// coalesced
		a_reg[i] = a[i * 16 + tid];			// coalesced

		// *** FIX: Transpose load for B = W^T ***
		//b_reg[i] = as_int((half2)(
		//	b[tid * 16 + 2*i],					// W[tid, 2*i] is loaded into B[2*i, tid]
		//	b[tid * 16 + 2*i + 1]));			// W[tid, 2*i + 1] is loaded into B[2*i + 1, tid]
	}

	b_reg = vload8(tid, (int*) b);

	c_reg = intel_sub_group_f16_f16_matrix_mad_k16(as_short8(a_reg), b_reg, c_reg);
	
	for(int i = 0; i < 8; i ++)
	{
		c[i * 16 + tid] = c_reg[i];			// coalesced
	}
}