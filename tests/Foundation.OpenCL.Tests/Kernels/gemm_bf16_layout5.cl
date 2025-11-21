


__attribute__((intel_reqd_sub_group_size(32)))
__kernel void gemm_bf16_layout5(
    __global half* y,
    __global half* x,
    __global half* w,
    int strideY,
    int strideX,
    int strideW,
    int batchSize)
{
    const int maxBatchSize = 256;
    //const int maxUnroll = 8;
    //const int warpSize = (int) get_sub_group_size();    //   16      32
    const int warpSize = 32;    //   16      32
    const int maxUnroll = 4;
    const int cacheLine = 256;                          //  256     256
    const int tileSize = 128;     //  128     128
    const int unroll = tileSize / warpSize;             //    8       4
    
    const int synchro = 1;

    const int tid = get_sub_group_local_id() % warpSize;
    const int sj = get_sub_group_id () * tileSize;

    half acc [maxBatchSize][maxUnroll];

    const int inputDim = strideX;
    
    half wjk [maxUnroll];

    for (int i = 0; i < batchSize; i ++)
    {
        #pragma unroll 
        for(int uj = 0; uj < unroll; uj ++)
            acc[i][uj] = 0.0h;
    }

    // Y[ i, j ] = Y[ i, j ] +  Sum_k X[ i, k ] W[ j, k ]

    for (int uj = 0; uj < tileSize; uj ++)
    {
        const int j = sj + uj;

        for(int sk = 0; sk < inputDim ; sk += tileSize )
        {
            if ((sk / tileSize) % synchro == synchro - 1)           // non divergent 
            {
                barrier(CLK_LOCAL_MEM_FENCE);

                // this way all the subgroups access the same portion of X, helps with locality.
                // Note : probably overkill
            }

            #pragma unroll 
            for (int uk = 0; uk < unroll ; uk ++)                   // reads a whole cache line
            {
                const int k = sk + uk * warpSize + tid;
                wjk[uk] = w[j * strideW + k];                       // coalesced.
            }
            
            for(int i = 0; i < batchSize ; i ++)
            {
                half sum = 0.0h;        // could be F32 for precision

                #pragma unroll 
                for (int uk = 0; uk < unroll ; uk ++)               // reads a whole cache line
                {
                    const int k = sk + uk * warpSize + tid;
                    half xik = x[i * strideX + k];                  // coalesced.
                    sum += xik * wjk[uk];
                }

                sum = sub_group_reduce_add(sum);                    // horizontal sum

                if (uj % warpSize == tid)                           // divergent
                    acc[i][uj / warpSize] += sum;
            }
        }
    }

    for (int i = 0; i < batchSize ; i ++)
    {
        #pragma unroll 
        for (int uj = 0; uj < unroll ; uj ++)                       // writes a whole cache line
        {
            const int j = sj + uj * warpSize + tid;
            y[i * strideY + j] += acc[i][uj];                       // coalesced.
        }
    }
}