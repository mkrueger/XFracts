using System;

using Android.Opengl;

using Java.Nio;

namespace XFracts
{
	class FractalShaderRenderer : IDisposable
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
			@"#ifdef GL_FRAGMENT_PRECISION_HIGH
precision highp float;
#else
precision mediump float;
#endif

uniform vec2 center;
uniform float scale;
uniform float ratio;
uniform vec4 palette[" + Options.PaletteSize + @"];
varying vec2 v_texcoord; ";
// 

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

		int vertexStride = COORDS_PER_VERTEX * 4;
		public float Ratio {
			get;
			set;
		}

 		// 4 bytes per vertex
		FractalGLSurfaceView context;
		public FractalShaderRenderer (FractalGLSurfaceView context)
		{
			this.context = context;
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


			CompileProgram ();             

			paletteBuffer = GeneratePaletteTexture ();
			Options.PaletteChanged += delegate {
				paletteBuffer = GeneratePaletteTexture ();
			};
			Options.FractalChanged += delegate {
				GLES20.GlDeleteProgram (program);
				GLES20.GlDeleteShader (vertexShader);
				GLES20.GlDeleteShader (fragmentShader);

				CompileProgram ();
			};
		}


		int vertexShader, fragmentShader;
		void CompileProgram ()
		{
			// prepare shaders and OpenGL program
			vertexShader = FractalGLSurfaceView.LoadShader (GLES20.GlVertexShader,
			                                            vertexShaderCode);
			fragmentShader = FractalGLSurfaceView.LoadShader (GLES20.GlFragmentShader,
			                                          fragmentShaderCode + Options.Fractal.FragmentShader);

			program = GLES20.GlCreateProgram ();
			// create empty OpenGL Program
			GLES20.GlAttachShader (program, vertexShader);
			// add the vertex shader to program
			GLES20.GlAttachShader (program, fragmentShader);
			// add the fragment shader to program
			GLES20.GlLinkProgram (program);

			// create OpenGL program executables
			positionHandle = GLES20.GlGetAttribLocation (program, "vPosition");
			textureHandle = GLES20.GlGetAttribLocation (program, "texcoord");
		}

		FloatBuffer paletteBuffer;
		
		FloatBuffer GeneratePaletteTexture ()
		{
			float[] pixels = Options.GeneratePalette ();
			
			ByteBuffer bb = ByteBuffer.AllocateDirect (pixels.Length * 4);
			bb.Order (ByteOrder.NativeOrder ());
			
			var buf = bb.AsFloatBuffer ();
			buf.Put (pixels);
			buf.Position (0);
			return buf;
		}

		public void Dispose ()
		{
			GLES20.GlDeleteProgram (program);
		}

		public void Draw (float[] mvpMatrix)
		{
			// Add program to OpenGL environment
			GLES20.GlUseProgram (program);
			FractalGLSurfaceView.CheckGlError ("glUseProgram");

			// Enable a handle to the triangle vertices
			GLES20.GlEnableVertexAttribArray (positionHandle);
			// Prepare the triangle coordinate data
			GLES20.GlVertexAttribPointer (positionHandle, COORDS_PER_VERTEX,
			                              GLES20.GlFloat, false,
			                              vertexStride, vertexBuffer);
			FractalGLSurfaceView.CheckGlError ("gvPositionHandle");

			GLES20.GlEnableVertexAttribArray (textureHandle);

			GLES20.GlVertexAttribPointer (textureHandle, 2, GLES20.GlFloat, false, 2 * 4, textCoordBuffer);
			FractalGLSurfaceView.CheckGlError ("glVertexAttribPointer");

			// set variables !
			var centerHandle = GLES20.GlGetUniformLocation (program, "center");
			GLES20.GlUniform2f (centerHandle, Options.X, Options.Y);
			// get handle to shape's transformation matrix
			FractalGLSurfaceView.CheckGlError ("glGetUniformLocation");

			var scaleHandle = GLES20.GlGetUniformLocation (program, "scale");
			GLES20.GlUniform1f (scaleHandle, Options.Scale);
			// get handle to shape's transformation matrix
			FractalGLSurfaceView.CheckGlError ("glGetUniformLocation");

			var ratioHandle = GLES20.GlGetUniformLocation (program, "ratio");
			GLES20.GlUniform1f (ratioHandle, Ratio);
			// get handle to shape's transformation matrix
			FractalGLSurfaceView.CheckGlError ("glGetUniformLocation");


			int paletteHandle = GLES20.GlGetUniformLocation (program, "palette");
			GLES20.GlEnableVertexAttribArray (paletteHandle);
			GLES20.GlVertexAttribPointer (paletteHandle, Options.PaletteSize, GLES20.GlFloat, false, 4, paletteBuffer);
			FractalGLSurfaceView.CheckGlError ("glVertexAttribPointer");


			// Apply the projection and view transformation
			var matrixHandle = GLES20.GlGetUniformLocation (program, "uMVPMatrix");
			GLES20.GlUniformMatrix4fv (matrixHandle, 1, false, mvpMatrix, 0);
			FractalGLSurfaceView.CheckGlError ("glUniformMatrix4fv");

			// Draw the square
			GLES20.GlDrawElements (GLES20.GlTriangles, drawOrder.Length,
			                      GLES20.GlUnsignedShort, drawListBuffer);

			// Disable vertex array
			GLES20.GlDisableVertexAttribArray (positionHandle);
			GLES20.GlDisableVertexAttribArray (paletteHandle);
		}
	}
}

