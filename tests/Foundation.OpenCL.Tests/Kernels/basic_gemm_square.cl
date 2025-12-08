
enum { k_size = 32 };

__attribute__((intel_reqd_sub_group_size(32)))
__kernel void basic_gemm_square(
    __global half* y,
    __global const half* x,
    __global const half* w)
{
    __local half c [k_size][k_size];
    __local half a [k_size][k_size];
    __local half b [k_size][k_size];
    
    event_t completion[2];

    completion[0] = async_work_group_copy(&a[0][0], x, k_size * k_size, 0);
    completion[1] = async_work_group_copy(&b[0][0], w, k_size * k_size, 0);

    wait_group_events(2, completion);

    const int tid = get_sub_group_local_id();
    
    event_t row_completion[k_size];

    for(int i = 0; i < k_size; i ++)
    {
        half res = 0.0h;
        for(int j = 0; j < k_size; j ++)
        {
            half sum = a[i][tid] * b[j][tid];
            sum = sub_group_reduce_add(sum);
        
            if (j == tid) res = sum;
        }

        c[i][tid] = res;

        row_completion[i] = async_work_group_copy(y, &c[0][0], k_size * k_size, 0);
    }

    wait_group_events(k_size, row_completion);
}