#version 330
layout (location = 0) in vec3 iPosition;
layout (location = 1) in vec3 iNormal;
layout (location = 2) in vec4 iColor;

out vec3 normal;
out vec4 color;
out vec3 position;

uniform mat4 model;
uniform mat4 proj;
uniform mat4 view;
uniform vec3 cameraPosition;

void main()
{
    vec4 pos = model * vec4(iPosition, 1.0);
    vec4 norm = model * vec4(iNormal, 0.0);

    normal = norm.xyz;
    color = iColor;
    position = pos.xyz;

    gl_Position = proj * view * pos;
}