using System;

using Android.Opengl;

using Java.Nio;

namespace XFracts
{
	class FractalRenderer : IDisposable
	{
		static string vertexShaderCode =
@"uniform mat4 uMVPMatrix;
attribute vec4 vPosition;
attribute vec2 texcoord;
varying vec2 v_texcoord;

void main() {
  gl_Position = vPosition * uMVPMatrix;
  v_texcoord = texcoord;
}";

		string fragmentShaderCode =
			@"precision mediump float;

uniform vec2 center;
uniform float scale;
varying vec2 v_texcoord;  

void main() {
	int n;
    
	float i0 = (v_texcoord.x - 0.5) * scale - center.x;
	float r0 = (v_texcoord.y - 0.5) * scale - center.y;

   	float i = i0;
    float r = r0;

    for(n=0; n<16; n++) {
        float x = (i * i - r * r) + i0;
        float y = (r * i + i * r) + r0;

        if((x * x + y * y) > 4.0) break;
        i = x;
        r = y;
    }

	gl_FragColor = vec4(0, 0, (float(n) / 16.0), 1.0);
}";
// gl_FragColor = texture2D(tex, vec2(n, 0));

		FloatBuffer vertexBuffer;
		FloatBuffer textCoordBuffer;
		ShortBuffer drawListBuffer;
		int program;
		int positionHandle;
		int textureHandle;

		static int COORDS_PER_VERTEX = 3;
		static float[] squareCoords = new float[] { 
			-1f,  1f, 0.0f,   // top left
			-1f, -1f, 0.0f,   // bottom left
			1f, -1f, 0.0f,    // bottom right
			1f, 1f, 0.0f };  // top right

		short[] drawOrder = new short[] { 
			0, 
			1, 
			2, 
			0, 
			2, 
			3
		}; // order to draw vertices

		float[] textureVertices = {
			0.0f, 0.0f,
			0.0f, 1.0f,
			1.0f, 1.0f,
			1.0f, 0.0f
		};

		int vertexStride = COORDS_PER_VERTEX * 4; // 4 bytes per vertex


		public FractalRenderer ()
		{
			// initialize vertex byte buffer for shape coordinates
			ByteBuffer bb = ByteBuffer.AllocateDirect (
				// (# of coordinate values * 4 bytes per float)
				squareCoords.Length * 4);
			bb.Order (ByteOrder.NativeOrder ());
			vertexBuffer = bb.AsFloatBuffer ();
			vertexBuffer.Put (squareCoords);
			vertexBuffer.Position (0);

			ByteBuffer bb2 = ByteBuffer.AllocateDirect (
				// (# of coordinate values * 4 bytes per float)
				textureVertices.Length * 4);
			bb2.Order (ByteOrder.NativeOrder ());
			textCoordBuffer = bb2.AsFloatBuffer ();
			textCoordBuffer.Put (textureVertices);
			textCoordBuffer.Position (0);

			// initialize byte buffer for the draw list
			ByteBuffer dlb = ByteBuffer.AllocateDirect (
			// (# of coordinate values * 2 bytes per short)
			        drawOrder.Length * 2);
			dlb.Order (ByteOrder.NativeOrder ());
			drawListBuffer = dlb.AsShortBuffer ();
			drawListBuffer.Put (drawOrder);
			drawListBuffer.Position (0);

			// prepare shaders and OpenGL program
			int vertexShader = MyGLRenderer.LoadShader (GLES20.GlVertexShader,
			                                           vertexShaderCode);
			int fragmentShader = MyGLRenderer.LoadShader (GLES20.GlFragmentShader,
			                                             fragmentShaderCode);

			program = GLES20.GlCreateProgram ();             // create empty OpenGL Program
			GLES20.GlAttachShader (program, vertexShader);   // add the vertex shader to program
			GLES20.GlAttachShader (program, fragmentShader); // add the fragment shader to program

			GLES20.GlLinkProgram (program);                  // create OpenGL program executables

			positionHandle = GLES20.GlGetAttribLocation (program, "vPosition");
			textureHandle = GLES20.GlGetAttribLocation (program, "texcoord");

		//	paletteTexture = GeneratePaletteTexture ();
		}


		int paletteTexture;

		int GeneratePaletteTexture ()
		{
			int[] textureIds = new int[1];
			GLES20.GlPixelStorei (GLES20.GlUnpackAlignment, 1);

			GLES20.GlGenTextures (1, textureIds, 0);
			GLES20.GlBindTexture (GLES20.GlTexture2d, textureIds[0]);
			
			byte[] pixels = new byte [32 * 4];
			
			for (int i = 0; i < pixels.Length; i++) {
				pixels[i] = (byte)i;
			}
			
			
			
			var buf = ByteBuffer.Allocate (pixels.Length);
			buf.Put (pixels);
			buf.Position (0);
			GLES20.GlTexImage2D ((int)GLES20.GlTexture2d, 0, (int)GLES20.GlRgba, 32, 1, 0, (int)GLES20.GlRgba, (int)GLES20.GlUnsignedByte, buf);

			GLES20.GlTexParameteri ( GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlNearest);
			GLES20.GlTexParameteri ( GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlNearest);

			return textureIds [0];
		}

		public void Dispose ()
		{
			GLES20.GlDeleteProgram (program);
		}

		public void Draw (float[] mvpMatrix)
		{
			// Add program to OpenGL environment
			GLES20.GlUseProgram (program);
			MyGLRenderer.CheckGlError ("glUseProgram");

			// Enable a handle to the triangle vertices
			GLES20.GlEnableVertexAttribArray (positionHandle);
			// Prepare the triangle coordinate data
			GLES20.GlVertexAttribPointer (positionHandle, COORDS_PER_VERTEX,
			                              GLES20.GlFloat, false,
			                              vertexStride, vertexBuffer);
			MyGLRenderer.CheckGlError ("gvPositionHandle");

			GLES20.GlEnableVertexAttribArray (textureHandle);

			GLES20.GlVertexAttribPointer (textureHandle, 2, GLES20.GlFloat, false, 2 * 4, textCoordBuffer);
			MyGLRenderer.CheckGlError ("gvTexCoordHandle");

			// set variables !
			var centerHandle = GLES20.GlGetUniformLocation (program, "center");
			GLES20.GlUniform2f (centerHandle, Options.X, Options.Y);
			// get handle to shape's transformation matrix
			MyGLRenderer.CheckGlError ("glGetUniformLocation");

			var scaleHandle = GLES20.GlGetUniformLocation (program, "scale");
			GLES20.GlUniform1f (scaleHandle, Options.Scale);
			// get handle to shape's transformation matrix
			MyGLRenderer.CheckGlError ("glGetUniformLocation");


			/*
			GLES20.GlActiveTexture (GLES20.GlTexture0);
			MyGLRenderer.CheckGlError ("GlActiveTexture");
			GLES20.GlBindTexture (GLES20.GlTexture2d, paletteTexture);
			MyGLRenderer.CheckGlError ("GlBindTexture");

			int textureHandler;
			textureHandler = GLES20.GlGetUniformLocation (program, "tex");
			GLES20.GlUniform1i (textureHandler, paletteTexture);
			MyGLRenderer.CheckGlError ("GlUniform1i");
*/

			// Apply the projection and view transformation
			var matrixHandle = GLES20.GlGetUniformLocation (program, "uMVPMatrix");
			GLES20.GlUniformMatrix4fv (matrixHandle, 1, false, mvpMatrix, 0);
			MyGLRenderer.CheckGlError ("glUniformMatrix4fv");

			// Draw the square
			GLES20.GlDrawElements (GLES20.GlTriangles, drawOrder.Length,
			                      GLES20.GlUnsignedShort, drawListBuffer);

			// Disable vertex array
			GLES20.GlDisableVertexAttribArray (positionHandle);
		}
	}
}

