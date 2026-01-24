

__attribute__((intel_reqd_sub_group_size(16)))
kernel void gemm_1_2048_16n_fused_layer_norm_shuffled(
    global half* c,
    global const half* a,
    global const half* b)