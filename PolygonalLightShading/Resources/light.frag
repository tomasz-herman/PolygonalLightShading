#version 330
uniform float intensity;
uniform sampler2D tex;
uniform bool useTexture;
in vec2 texCoord;
in vec3 color;

void main()
{
    if(useTexture) {
        gl_FragColor = texture(tex, texCoord) * intensity;
    } else {
        gl_FragColor = vec4(intensity * color, 1);
    }
}