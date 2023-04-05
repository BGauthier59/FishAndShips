#ifndef OCEAN_ST_INCLUDE
#define OCEAN_ST_INCLUDE

float iTime;

float2x2 rot(float a){
    float c = cos(a);
    float s = sin(a);
    return float2x2(c, -s, s, c);
}

void WaveRender_float( float2 v, float time, float2 r, out float4 color)
{
    color = float4(0.0, 0.0, 0.0, 1.0);
    float e, i, a, w, x, g;
    for (;i++<1e2;)
    {
        float3 p = float3((v.xy-.5*r)/r.y*g, g-3.0);
        p.zy = mul(p.zy, rot(.6));
        if(i < 1e2) {p=p;} else {p = p+float3(0.0001, 0.0001, 0.0001);}
        e=p.y;
        for (a =0.8; a> 0.003; a*= 0.8)
        {
            p.xz = mul(p.xz, rot(5.0));
            x = (++p.x + p.z)/a+time+time,
            w = exp(sin(x) -2.5)*a,
            color.gb += w/1e2,
            p.xz -= w*cos(x),
            e-=w;
        }
        g+=e;
    }
    color +=min(e*e*4e6,1./g)+g*g/2e2;
}


#endif