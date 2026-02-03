

__attribute__((intel_reqd_sub_group_size(32)))
kernel void gemv_kiss(
    global const half* c,		// output_dim
    global const half* a,		// output_dim x input_dim
    global const half* b)		// input_dim
{
	// in that subgroup we compute c[i] = sum_k a[i,k] b[k]

	const int i = get_group_id(0);
	const int tid = get_sub_group_local_id();
	
	const int input_dim = 2048;
	const int output_dim = 2048;
	const int warp_size = 32;
	
	const int * pa = (int * )(a + i * input_dim);
	const int * pb = (int * )b;

	const int * last_b = (int*) (b+ input_dim);
	const int increment = 16*warp_size;
	
	private float acc0 = 0.0f;
	private float acc1 = 0.0f;

	__attribute__((opencl_unroll_hint(-1))		// disable unroll
	do
	{
		private int16 va = vload16(tid, pa);
		private int16 vb = vload16(tid, pb);
		
		acc0 += dot(as_half16(va.lo),as_half16(vb.lo));			// corrected : hi and lo in Khronos's spec
		acc1 += dot(as_half16(va.hi),as_half16(vb.hi));
		
		pa += increment;
		pb += increment;
	} while(pb < last_b);

	private float acc = sub_group_reduce_add(acc0 + acc1);

	if (tid == 0) c[i] = (half) acc;							// divergent write (correct)
}

__attribute__((intel_reqd_sub_group_size(32)))
kernel void gemm_kiss(
    global const half* c,		// output_dim x batch_size	(col major)
    global const half* a,		// output_dim x input_dim	(row major)
    global const half* b)		// input_dim x batch_size	(col major)
{
	// in that subgroup we compute c[i] = sum_k a[i,k] b[k]

	const int i = get_group_id(0);
	const int tid = get_sub_group_local_id();
	
	const int input_dim = 2048;
	const int output_dim = 2048;
	const int warp_size = 32;
	const int batch_size = 32;

	const int * pa = (int * )(a + i * input_dim);
	const int * pb = (int * )b;

	const int * last_b = (int*) (b+ input_dim);
	const int increment = 16*warp_size;

	private float acc[batch_size] = {0.0f};

	__attribute__((opencl_unroll_hint(-1))		// disable unroll
	do
	{
		private int16 va = vload16(tid, pa);

		for(int j = 0; j < batch_size ; j ++)
		{
			private int16 vb = vload16(tid, pb + j * input_dim);
		
			acc[j] += dot(as_half16(va.lo),as_half16(vb.lo));
			acc[j] += dot(as_half16(va.hi),as_half16(vb.hi));
		}

		pa += increment;
		pb += increment;
	} while(pb < last_b);

	for(int j = 0; j < batch_size ; j ++) acc[j] = sub_group_reduce_add(acc[j]);

	for(int k = 0; k < batch_size; k += warp_size)
	{
		if (tid + k <= batch_size) c[i + output_dim * (tid + k)] = (half) acc[tid + k];
	}
}

__attribute__((intel_reqd_sub_group_size(32)))
kernel void gemm_kiss_2(
    global const half* c,		// output_dim x batch_size	(col major)
    global const half* a,		// output_dim x input_dim	(row major)
    global const half* b)		// input_dim x batch_size	(col major)
{
	// in that subgroup we compute c[i] = sum_k a[i,k] b[k]

	const int i = get_group_id(0);
	const int tid = get_sub_group_local_id();
	
	const int input_dim = 2048;
	const int output_dim = 2048;
	const int warp_size = 32;
	const int batch_size = 32;

	const int * pa = (uint * )(a + i * input_dim);
	const int * pb = (uint * )b;

	const int * last_b = (uint*) (b+ input_dim);
	const int increment = 8 * warp_size;

	private float acc[batch_size] = {0.0f};

	__attribute__((opencl_unroll_hint(-1))		// disable unroll
	do
	{
		private uint8 va = intel_sub_group_block_read8(pa);

		for(int j = 0; j < batch_size ; j ++)
		{
			private uint8 vb = intel_sub_group_block_read8(pb + j * input_dim /2);
			acc[j] += dot(as_half16(va),as_half16(vb));
		}

		pa += increment;
		pb += increment;
	} while(pb < last_b);

	for(int j = 0; j < batch_size ; j ++) acc[j] = sub_group_reduce_add(acc[j]);

	for(int k = 0; k < batch_size; k += warp_size)
	{
		if (tid + k <= batch_size) c[i + output_dim * (tid + k)] = (half) acc[tid + k];
	}
}

__attribute__((intel_reqd_sub_group_size(32)))
kernel void gemm_fused_kiss(
    global const half* c,		// output_dim x batch_size	(col major)
    global const half* a,		// output_dim x input_dim	(row major)
    global const half* b)		// input_dim x batch_size	(col major)
{
	// in that subgroup we compute c[i, j] = sum_k a[i, k] b[k, j]

	const int i = get_group_id(0);      // Which output row
	const int tid = get_sub_group_local_id();
	
	const int input_dim = 2048;
	const int output_dim = 2048;
	const int warp_size = 32;
	const int batch_size = 32;

	const int * pa = (int * )(a + i * input_dim);
	const int * pb = (int * )b;

	const int * last_b = (int*) (b+ input_dim);
	const int increment = 16*warp_size;
	
	private float acc[batch_size];
	private float lyn[batch_size];
    for (int j = 0; j < 32; j++)
	{
		acc[j] = 0.0f;
		lyn[j] = 0.0f;
	}

	__attribute__((opencl_unroll_hint(-1))		// disable unroll
	do
	{
		private int16 va = vload16(tid, pa);

		for(int j = 0; j < batch_size ; j ++)
		{
			private int16 vb = vload16(tid, pb + j * input_dim);
		
			acc[j] += dot(as_half16(va.lo),as_half16(vb.lo));
			acc[j] += dot(as_half16(va.hi),as_half16(vb.hi));

			lyn[j] += dot(as_half16(vb.lo),as_half16(vb.lo));
			lyn[j] += dot(as_half16(vb.hi),as_half16(vb.hi));
		}

		pa += increment;
		pb += increment;
	} while(pb < last_b);

	for(int j = 0; j < batch_size ; j ++)
	{
		acc[j] = sub_group_reduce_add(acc[j]);
		lyn[j] = sub_group_reduce_add(lyn[j]);
	}

	for(int k = 0; k < batch_size; k += warp_size)
	{
		int j = k + tid;
		if (j >= batch_size) continue;
		c[i + output_dim * j] = (half) post_operation(acc[j] / (sqrt(lyn[j]) + 1e-6));
	}
}