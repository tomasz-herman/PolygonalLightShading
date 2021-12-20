#version 410 core

layout(vertices = 16) out;

uniform vec3 cameraPos;

float factor(vec4 v1, vec4 v2, vec4 v3, vec4 v4) {
    vec3 v = 0.25 * (v1 + v2 + v3 + v4).xyz;
    float dist = distance(v, cameraPos);
    return -16 * log(dist * 0.01) / log(10);
}

void main() {
    gl_out[ gl_InvocationID ].gl_Position = gl_in[ gl_InvocationID ].gl_Position;
    gl_TessLevelOuter[0] = factor(gl_in[ 0].gl_Position, gl_in[ 4].gl_Position, gl_in[ 8].gl_Position, gl_in[12].gl_Position);
    gl_TessLevelOuter[2] = factor(gl_in[ 3].gl_Position, gl_in[ 7].gl_Position, gl_in[11].gl_Position, gl_in[15].gl_Position);
    gl_TessLevelOuter[1] = factor(gl_in[ 0].gl_Position, gl_in[ 1].gl_Position, gl_in[ 2].gl_Position, gl_in[3].gl_Position);
    gl_TessLevelOuter[3] = factor(gl_in[12].gl_Position, gl_in[13].gl_Position, gl_in[14].gl_Position, gl_in[15].gl_Position);
    gl_TessLevelInner[0] = 0.5 * (gl_TessLevelOuter[1] + gl_TessLevelOuter[3]);
    gl_TessLevelInner[1] = 0.5 * (gl_TessLevelOuter[0] + gl_TessLevelOuter[2]);
}