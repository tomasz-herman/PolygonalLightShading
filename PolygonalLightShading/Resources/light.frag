#version 330
uniform float intensity;
uniform sampler2D tex;
uniform bool useTexture;
in vec2 texCoord;
in vec3 color;

void main()
{
    if(useTexture) {
        gl_FragColor = vec4(color * texture(tex, texCoord).xyz * min(intensity, 1), 1);
    } else {
        gl_FragColor = vec4(min(intensity, 1) * color, 1);
    }
}