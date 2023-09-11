#version 320 es

precision highp float;

out vec4 out_Color;

uniform vec4 u_EdgeColor;

void main()
{
	out_Color = u_EdgeColor;
}
