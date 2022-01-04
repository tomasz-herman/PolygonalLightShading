#version 330
uniform float intensity;
uniform vec3 ambient;

in vec3 color;

void main()
{
    gl_FragColor = vec4((vec3(intensity, intensity, intensity) + ambient) * color, 1);
}