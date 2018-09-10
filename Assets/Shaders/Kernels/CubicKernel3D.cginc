#ifndef CubicKernel3D
#define CubicKernel3D

#define PI 3.1415926535897932384626433832795

#define FPType float
#define FPType3 float3

cbuffer CubicKernel {
    FPType W_zero;
    FPType K;
    FPType L;
    FPType Radius;
    FPType InvRadius;
};

void InitCubicKernel3D(FPType radius);
FPType W(FPType x, FPType y, FPType z);
FPType W(FPType3 vec);
FPType W_ZERO();
FPType3 GradW(FPType x, FPType y, FPType z);
FPType3 GradW(FPType3 vec);

void InitCubicKernel3D(FPType radius){
    Radius = radius;
    InvRadius = 1.0 / radius;

    FPType h3 = radius * radius * radius;

    K = 8.0 / (PI * h3);
    L = 48.0 / (PI * h3);

    W_zero = W(0,0,0);
}

inline FPType W(FPType x, FPType y, FPType z)
{
    FPType res = 0.0;
    FPType rl = sqrt(x * x + y * y + z * z);
    FPType q = rl * InvRadius;

    if (q <= 1.0)
    {
        if (q <= 0.5)
        {
            FPType q2 = q * q;
            FPType q3 = q2 * q;
            res = K * (6.0 * q3 - 6.0f * q2 + 1.0);
        }
        else
        {
            FPType v = 1.0 - q;
            res = K * 2.0 * v*v*v;
        }
    }
    return res;
}

inline FPType W(FPType3 vec){
    return W(vec.x,vec.y,vec.z);
}

inline FPType W_ZERO(){
    return W(0,0,0);
}


inline FPType3 GradW(FPType x, FPType y, FPType z)
{

    FPType3 res = FPType3(0,0,0);
    FPType rl = sqrt(x*x + y*y + z*z);
    FPType q = rl * InvRadius;
    FPType factor;

    if (q <= 1.0)
    {
        if (rl > 1.0e-6)
        {
            FPType3 gradq;

            factor = 1.0 / (rl * Radius);

            gradq.x = x * factor;
            gradq.y = y * factor;
            gradq.z = z * factor;

            if (q <= 0.5)
            {
                factor = L * q * (3.0 * q - 2.0);

                res.x = gradq.x * factor;
                res.y = gradq.y * factor;
                res.z = gradq.z * factor;
            }
            else
            {
                factor = 1.0 - q;
                factor = L * (-factor * factor);

                res.x = gradq.x * factor;
                res.y = gradq.y * factor;
                res.z = gradq.z * factor;
            }
        }
    }

    return res;
}

inline FPType3 GradW(FPType3 vec){
    return GradW(vec.x,vec.y,vec.z);
}

#endif