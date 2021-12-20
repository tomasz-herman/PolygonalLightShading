#version 410 core

layout(quads, fractional_odd_spacing, ccw) in;

uniform mat4 mvp;
uniform vec3 cameraPos;

uniform sampler2D heightTex;

out vec3 position;
out vec3 tangent;
out vec3 bitangent;
out vec3 normal;
out vec2 uv;

float mipLevel(vec3 v) {
    float dist = distance(v, cameraPos);
    float factor = -16 * log(dist * 0.01) / log(10);
    return 6 - log(factor);
}

void main() {
    vec4 p00 = gl_in[ 0].gl_Position;
    vec4 p10 = gl_in[ 1].gl_Position;
    vec4 p20 = gl_in[ 2].gl_Position;
    vec4 p30 = gl_in[ 3].gl_Position;
    vec4 p01 = gl_in[ 4].gl_Position;
    vec4 p11 = gl_in[ 5].gl_Position;
    vec4 p21 = gl_in[ 6].gl_Position;
    vec4 p31 = gl_in[ 7].gl_Position;
    vec4 p02 = gl_in[ 8].gl_Position;
    vec4 p12 = gl_in[ 9].gl_Position;
    vec4 p22 = gl_in[10].gl_Position;
    vec4 p32 = gl_in[11].gl_Position;
    vec4 p03 = gl_in[12].gl_Position;
    vec4 p13 = gl_in[13].gl_Position;
    vec4 p23 = gl_in[14].gl_Position;
    vec4 p33 = gl_in[15].gl_Position;
    float u = gl_TessCoord.x;
    float v = gl_TessCoord.y;

    float bu0 = (1.-u) * (1.-u) * (1.-u);
    float bu1 = 3. * u * (1.-u) * (1.-u);
    float bu2 = 3. * u * u * (1.-u);
    float bu3 = u * u * u;
    float dbu0 = -3. * (1.-u) * (1.-u);
    float dbu1 =  3. * (1.-u) * (1.-3.*u);
    float dbu2 =  3. * u *      (2.-3.*u);
    float dbu3 =  3. * u *      u;
    float bv0 = (1.-v) * (1.-v) * (1.-v);
    float bv1 = 3. * v * (1.-v) * (1.-v);
    float bv2 = 3. * v * v * (1.-v);
    float bv3 = v * v * v;
    float dbv0 = -3. * (1.-v) * (1.-v);
    float dbv1 =  3. * (1.-v) * (1.-3.*v);
    float dbv2 =  3. * v *      (2.-3.*v);
    float dbv3 =  3. * v *      v;


    position =
    ( bu0 * ( bv0*p00 + bv1*p01 + bv2*p02 + bv3*p03 )
    + bu1 * ( bv0*p10 + bv1*p11 + bv2*p12 + bv3*p13 )
    + bu2 * ( bv0*p20 + bv1*p21 + bv2*p22 + bv3*p23 )
    + bu3 * ( bv0*p30 + bv1*p31 + bv2*p32 + bv3*p33 )).xyz;
    
    uv = position.xz / 12 + 0.5;
    float height = textureLod(heightTex, uv, mipLevel(position)).r * 0.75;
    height *= height;

    tangent = normalize
    ((dbu0 * ( bv0*p00 + bv1*p01 + bv2*p02 + bv3*p03 )
    + dbu1 * ( bv0*p10 + bv1*p11 + bv2*p12 + bv3*p13 )
    + dbu2 * ( bv0*p20 + bv1*p21 + bv2*p22 + bv3*p23 )
    + dbu3 * ( bv0*p30 + bv1*p31 + bv2*p32 + bv3*p33 )).xyz);

    bitangent = normalize
    ((dbv0 * ( bu0*p00 + bu1*p10 + bu2*p20 + bu3*p30 )
    + dbv1 * ( bu0*p01 + bu1*p11 + bu2*p21 + bu3*p31 )
    + dbv2 * ( bu0*p02 + bu1*p12 + bu2*p22 + bu3*p32 )
    + dbv3 * ( bu0*p03 + bu1*p13 + bu2*p23 + bu3*p33 )).xyz);

    normal = normalize(cross(tangent, bitangent));

    position += height * normal;
    gl_Position = mvp * vec4(position, 1.0f);
}
