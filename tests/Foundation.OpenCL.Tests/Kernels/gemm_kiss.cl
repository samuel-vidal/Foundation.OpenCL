

#define __double_buffer

__attribute__((intel_reqd_sub_group_size(32)))
kernel void gemm_kiss(
    global half* c,				// output_dim x batch_size	(col major)
    global const half* a,		// output_dim x input_dim	(row major)
    global const half* b,		// input_dim x batch_size	(col major)
	int batch_size)
{
	// in that subgroup we compute c[i] = sum_k a[i,k] b[k]

	const int i = get_group_id(0);
	const int tid = get_sub_group_local_id();
	
	const int input_dim = 2048;
	const int output_dim = 2048;
	const int warp_size = 32;
	const int max_batch_size = 32;

	const global uint * pa = (global uint * )(a + i * input_dim);
	const global uint * pb = (global uint * )b;

	const uint * last_a = (global uint*) (a+ (i + 1) *input_dim);
	const int increment = 8 * warp_size;

	private float acc[max_batch_size];

	for(int j = 0; j < batch_size; j ++) acc[j] = 0.0f;

#ifdef __double_buffer
	private uint8 va = intel_sub_group_block_read8(pa);
#endif

	do
	{

	
#ifndef __double_buffer
		private uint8 va = intel_sub_group_block_read8(pa);
#endif

		half16 weights = as_half16(va);

#ifdef __double_buffer
		if (pa + increment < last_a) va = intel_sub_group_block_read8(pa + increment);
#endif

		for(int j = 0; j < batch_size ; j ++)
		{
			private uint8 vb = intel_sub_group_block_read8(pb + j * input_dim /2);
			half16 activation = as_half16(vb);

			float d = dot(weights.s0123, activation.s0123);
			d += dot(weights.s4567, activation.s4567);
			d += dot(weights.s89ab, activation.s89ab);
			d += dot(weights.scdef, activation.scdef);

			acc[j] += d;
		}

		pa += increment;
		pb += increment;
	} while(pa < last_a);

	for(int j = 0; j < batch_size ; j ++) acc[j] = sub_group_reduce_add(acc[j]);

	for(int k = 0; k < batch_size; k += warp_size)
	{
		if (tid + k <= batch_size) c[i + output_dim * (tid + k)] = (half) acc[tid + k];
	}
}