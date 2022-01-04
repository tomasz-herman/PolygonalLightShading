#version 330
uniform float intensity;
in vec3 color;

void main()
{
    gl_FragColor = vec4(intensity * color, 1);
}