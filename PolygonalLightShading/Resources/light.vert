#version 330
layout (location = 0) in vec3 iPosition;
//layout (location = 1) in vec3 iNormal;
//layout (location = 2) in vec4 iColor;
layout (location = 3) in vec2 iTexCoord;

out vec2 texCoord;
out vec3 color;

uniform vec3 lightColor;
uniform mat4 model;
uniform mat4 proj;
uniform mat4 view;

void main()
{
    gl_Position = proj * view * model * vec4(iPosition, 1.0);
    color = lightColor;
    texCoord = iTexCoord;
}