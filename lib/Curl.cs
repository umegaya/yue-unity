using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Net;

namespace Yue
{
	public class Curl : System.IDisposable
	{
		private HttpWebRequest request;
		private System.IO.Stream responseStream;
		private byte [] readWorkMemory;
		private System.IO.MemoryStream responseData;
		private bool requestFinished;
		private System.Exception exception;
		
		static Curl() {
			/*  certificateが不正でもそのまま続行する（ただ暗号化したいだけなので問題ない） */
			ServicePointManager.ServerCertificateValidationCallback = delegate { 
				return true; 
			};
		}
		
		public Curl(string url = null, byte[] data = null, Dictionary<string, object> headers = null)
		{
			this.responseData = new System.IO.MemoryStream();
			this.readWorkMemory = new byte[1024];
			this.requestFinished = false;
			if (url != null) {
				this.Open(url, data, headers);
			}
		}
		
		public void Dispose() {
			if (this.responseStream != null) {
				this.responseStream.Close();
			}
			if (this.responseData != null) {
				this.responseData.Close();
			}
		}
		
		public byte[] bytes {
			get {
				return this.responseData.ToArray();
			}
		}
		public int status {
			get; set; 
		}
		public string error {
			get {
				return this.exception != null ? this.exception.ToString() : null;
			}
		}
		public bool isDone {
			get {
				return this.requestFinished;
			}
		}
		
		public void Open(string url, byte[] data, Dictionary<string, object> headers) {
			var req = (HttpWebRequest)WebRequest.Create(url);
			this.request = req;
			req.ReadWriteTimeout = 100000; //10sec timeout
			req.Method = "POST";
			req.ContentType = "application/octet-stream";
			req.ContentLength = data.Length;
			/*  ヘッダーの設定 */
			if (headers != null) {
				foreach (KeyValuePair<string, object> header in headers) {
					req.Headers.Add(header.Key, header.Value.ToString());
				}
			}
			/*  ポスト・データの書き込み */
			System.IO.Stream reqStream = req.GetRequestStream();
			reqStream.Write(data, 0, data.Length);
			reqStream.Close();
			/*  response 取得 */
			req.BeginGetResponse(new System.AsyncCallback(ResponseCallback), this);
		}
		
		public System.IO.MemoryStream response()
		{
			if (this.requestFinished) {
				return this.responseData;
			}
			else {
				return null;
			}
		}
		
		private static void ResponseCallback(System.IAsyncResult ar)
		{
			/* 状態オブジェクトとしてわたされたCurlを取得 */
			Curl www = (Curl) ar.AsyncState;
			
			try {
				/* 非同期要求を終了 */
				System.Net.HttpWebResponse res =
					(System.Net.HttpWebResponse) www.request.EndGetResponse(ar);

				/* status code */
				www.status = (int)res.StatusCode;
					
				/* 読み出し用ストリームを取得 */
				www.responseStream = res.GetResponseStream();
				
				/* 非同期でデータの読み込みを開始 */
				www.responseStream.BeginRead(www.readWorkMemory, 0, www.readWorkMemory.Length,
					new System.AsyncCallback(ReadCallback), www);
			}
			catch (System.Exception e) {
				www.exception = e;
				www.requestFinished = true;
			}
		}
		
		/* 非同期読み込み完了時に呼び出されるコールバックメソッド */
		private static void ReadCallback(System.IAsyncResult ar)
		{
			/* 状態オブジェクトとしてわたされたCurlを取得 */
			Curl www = (Curl) ar.AsyncState;

			try {
				/* データを読み込む */
				int readSize = www.responseStream.EndRead(ar);
				
				if (readSize > 0)
				{
					/* データが読み込めた時 */
					/* 読み込んだデータをMemoryStreamに保存する */
					www.responseData.Write(www.readWorkMemory, 0, readSize);
					
					/* 再び非同期でデータを読み込む */
					www.responseStream.BeginRead(www.readWorkMemory, 0, www.readWorkMemory.Length, 
						new System.AsyncCallback(ReadCallback), www);
				}
				else
				{
					/* データの読み込みが終了した時 */
					/* 閉じる */
					www.responseStream.Close();
					www.responseStream = null;
					www.requestFinished = true;
				}
			}
			catch (System.Exception e) {
				www.exception = e;
				www.requestFinished = true;
			}
		}
	};
}
