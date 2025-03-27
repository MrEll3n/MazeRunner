#version 330 core

in vec3 vColor;         // interpolovane barvy z VS
out vec4 outColor;      // vystup musi byt vec4

void main() {
    outColor = vec4(vColor, 1.0);
}