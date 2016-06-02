
using Android.Views;
using Android.Content;
using Android.Opengl;
using Android.Graphics;
using Java.Nio;
using Android.Util;
using Java.Lang;

namespace XFracts
{
	class FractalGLSurfaceView : GLSurfaceView, GLSurfaceView.IRenderer
	{
		ScaleGestureDetector mScaleDetector;
		const float TOUCH_SCALE_FACTOR = 180.0f / 320;

		public class ScaleListener : ScaleGestureDetector.SimpleOnScaleGestureListener
		{
			public override bool OnScale(ScaleGestureDetector detector)
			{
				Options.Scale /= detector.ScaleFactor;

				return true;
			}
		}

		public FractalGLSurfaceView (Context context) : base (context)
		{
			// Create an OpenGL ES 2.0 context.
			SetEGLContextClientVersion (2);

			// Set the Renderer for drawing on the GLSurfaceView
			SetRenderer (this);

			mScaleDetector = new ScaleGestureDetector (context, new ScaleListener ());
			// Render the view only when there is a change in the drawing data
			this.RenderMode = Rendermode.WhenDirty;
		}

		float prevx;
		float prevy;

		public override bool OnTouchEvent (MotionEvent e)
		{
			base.OnTouchEvent (e);
			mScaleDetector.OnTouchEvent (e);

			if (e.Action == MotionEventActions.Down) {
				prevx = e.GetX ();
				prevy = e.GetY ();
			}

			if (e.Action == MotionEventActions.Move) {
				float e_x = e.GetX ();
				float e_y = e.GetY ();
				
				float xdiff = (prevx - e_x);
				float ydiff = (prevy - e_y);
				Options.X += xdiff * Options.Scale / 1024;
				Options.Y -= ydiff * Options.Scale / 1024;
				RequestRender ();

				prevx = e_x;
				prevy = e_y;
			}

			return true;
		}

		#region Renderer
		static string TAG = "MyGLRenderer";
		FractalShaderRenderer   mSquare;
		float[] mMVPMatrix = new float[16];
		float[] mProjMatrix = new float[16];
		float[] mVMatrix = new float[16];
		float[] mRotationMatrix = new float[16];

		bool takeScreenshot;
		
		public void TakeScreenshot ()
		{
			takeScreenshot = true;
			RequestRender ();
		}
		
		Bitmap GetScreenshot ()
		{
			using (var buf = Java.Nio.ByteBuffer.AllocateDirect (Width * Height * 4)) {
				buf.Order (Java.Nio.ByteOrder.NativeOrder ());

				GLES20.GlReadPixels (0, 0, Width, Height, GLES20.GlRgba, GLES20.GlUnsignedByte, buf);
				var data = new int[Width * Height];

				// Doesn't really work?
				//buf.AsIntBuffer ().Get (data);

				var bb = new byte[Width * Height * 4];
				buf.Get (bb);
				int o = 0;
				for (int i = 0; i < data.Length; i++) {
					data [i] =
						bb [o++] << 16 | 
						bb [o++] << 8  | 
						bb [o++]  |
						bb [o++] << 24;
				}
				

				return Bitmap.CreateBitmap (data, 0, Width, Width, Height, Bitmap.Config.Argb8888);
			}
		}
		
		#region IRenderer implementation
		public void OnDrawFrame (Javax.Microedition.Khronos.Opengles.IGL10 gl)
		{
			// Draw background color
			GLES20.GlClear (GLES20.GlColorBufferBit);
			
			// Set the camera position (View matrix)
			Android.Opengl.Matrix.SetLookAtM (mVMatrix, 0, 0, 0, -3, 0f, 0f, 0f, 0f, 1.0f, 0.0f);
			
			// Calculate the projection and view transformation
			Android.Opengl.Matrix.MultiplyMM (mMVPMatrix, 0, mProjMatrix, 0, mVMatrix, 0);
			
			// Draw square
			mSquare.Draw (mMVPMatrix);
			
			// Combine the rotation matrix with the projection and camera view
			Android.Opengl.Matrix.MultiplyMM (mMVPMatrix, 0, mRotationMatrix, 0, mMVPMatrix, 0);
			
			if (takeScreenshot) {
				var wallpaper = GetScreenshot ();
				System.Console.WriteLine ("got wallpaper:"+wallpaper);
				Android.App.WallpaperManager.GetInstance (Context).SetBitmap (wallpaper);
				takeScreenshot = false;
			}
			
		}
		
		public void OnSurfaceChanged (Javax.Microedition.Khronos.Opengles.IGL10 gl, int width, int height)
		{
			// Adjust the viewport based on geometry changes,
			// such as screen rotation
			GLES20.GlViewport (0, 0, width, height);
			
			// this projection matrix is applied to object coordinates
			// in the onDrawFrame() method
			mSquare.Ratio = width / (float)height;
			Android.Opengl.Matrix.FrustumM (mProjMatrix, 0, -1, 1, -1, 1, 3, 7);
		}
		
		public void OnSurfaceCreated (Javax.Microedition.Khronos.Opengles.IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
		{
			// Set the background frame color
			GLES20.GlClearColor (0.0f, 0.0f, 0.0f, 1.0f);
			
			mSquare = new FractalShaderRenderer (this);
		}
		#endregion
		
		public static int LoadShader (int type, string shaderCode)
		{
			// create a vertex shader type (GLES20.GL_VERTEX_SHADER)
			// or a fragment shader type (GLES20.GL_FRAGMENT_SHADER)
			int shader = GLES20.GlCreateShader (type);
			
			// add the source code to the shader and compile it
			GLES20.GlShaderSource (shader, shaderCode);
			GLES20.GlCompileShader (shader);
			
			return shader;
		}
		
		/**
		* Utility method for debugging OpenGL calls. Provide the name of the call
		* just after making it:
		*
		* <pre>
		* mColorHandle = GLES20.glGetUniformLocation(mProgram, "vColor");
		* MyGLRenderer.checkGlError("glGetUniformLocation");</pre>
		*
		* If the operation is not successful, the check throws an error.
		*
		* @param glOperation - Name of the OpenGL call to check.
		*/
		public static void CheckGlError (string glOperation)
		{
			int error;
			while ((error = GLES20.GlGetError ()) != GLES20.GlNoError) {
				Log.Error (TAG, glOperation + ": glError " + error);
				throw new RuntimeException (glOperation + ": glError " + error);
			}
		}
		#endregion
	}
}

