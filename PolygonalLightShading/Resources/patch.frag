#version 410 core
out vec4 FragColor;

uniform vec3 cameraPos;
uniform vec3 lightPos;

uniform sampler2D colorTex;
uniform sampler2D normalTex;

in vec3 position;
in vec3 tangent;
in vec3 bitangent;
in vec3 normal;
in vec2 uv;

void main() {
    vec3 N = normalize(texture(normalTex, uv).rgb * 2 - 1);
    vec3 n = normalize(N.x * normalize(tangent) + N.y * normalize(bitangent) + N.z * normalize(normal));
    
    vec3 lightDir = normalize(lightPos - position);
    float diff = max(dot(n, lightDir), 0.0);
    
    vec3 viewDir = normalize(cameraPos - position);
    vec3 reflectDir = reflect(-lightDir, n);
    float spec = 0.5 * pow(max(dot(viewDir, reflectDir), 0.0), 32);

    FragColor = vec4(min(texture(colorTex, uv).rgb * diff + vec3(1, 1, 1) * spec, 1), 1);
}