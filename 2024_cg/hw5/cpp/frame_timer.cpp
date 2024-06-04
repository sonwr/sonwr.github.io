/*
	How to use this code:

	Call init_timer before starting rendering, i.e., before calling
	glutMainLoop. Then, make sure your display function is organized
	roughly as the example below.
*/

#include <stdio.h>
#include <string.h>
#include <string>
#include <vector>
#include <queue>
#include <fstream>
#include <float.h>

#include <GL/glew.h>
#include <GL/glut.h>

#if defined(__APPLE__)
#include <OpenGL/gl.h>
#include <OpenGL/glu.h>
#include <OpenGL/glext.h>
#else
#include <GL/gl.h>
#include <GL/glu.h>
#include <GL/glext.h>
#endif


float  					gTotalTimeElapsed 	= 0;
int 					gTotalFrames		= 0;
GLuint 					gTimer;

void init_timer()
{
	glGenQueries(1, &gTimer);
}

void start_timing()
{
	glBeginQuery(GL_TIME_ELAPSED, gTimer);
}

float stop_timing()
{
	glEndQuery(GL_TIME_ELAPSED);

	GLint available = GL_FALSE;
	while (available == GL_FALSE)
		glGetQueryObjectiv(gTimer, GL_QUERY_RESULT_AVAILABLE, &available);

	GLint result;
	glGetQueryObjectiv(gTimer, GL_QUERY_RESULT, &result);

	float timeElapsed = result / (1000.0f * 1000.0f * 1000.0f);
	return timeElapsed;
}

/*
	Your display function should look roughly like the following.
*/
void display()
{
	// TODO: Clear the screen and depth buffer.

 	start_timing();

 	// TODO: Draw the bunny.
  	
	float timeElapsed = stop_timing();
  	gTotalFrames++;
  	gTotalTimeElapsed += timeElapsed;
  	float fps = gTotalFrames / gTotalTimeElapsed;
  	char string[1024] = {0};
  	sprintf(string, "OpenGL Bunny: %0.2f FPS", fps);
  	glutSetWindowTitle(string);

	glutPostRedisplay();
  	glutSwapBuffers();
}
