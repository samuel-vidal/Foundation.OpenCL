



__attribute__((intel_reqd_sub_group_size(16)))
__kernel void gemm_8_16_16_col_maj(
    __global half* y,
    __global const half* w,
    __global const half* x)
{
    const int tid = get_sub_group_local_id() % 16;

	__private short8 a_reg = vload8(tid, (short *)w);
	__private int8 b_reg = vload8(tid, (int*) x);
	__private half8 c_reg = vload8(tid, y);

	c_reg = intel_sub_group_f16_f16_matrix_mad_k16(a_reg, b_reg, c_reg);
	
	vstore8(c_reg, tid, y); 
}