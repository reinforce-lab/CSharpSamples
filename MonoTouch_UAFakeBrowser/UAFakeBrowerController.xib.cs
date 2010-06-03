
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using MonoTouch.Foundation;
        using MonoTouch.UIKit;
        using MonoTouch.ObjCRuntime;

        namespace UAFakeBrowser
        {
	        public partial class UAFakeBrowerController : UIViewController
	        {
                const String _startURL = "http://hootsuite.com/dashboard";
                //const String _startURL = "http://www.nihira.jp/cyber/UserAgentCheckTest.html";
                //const String _startURL = "http://whatsmyuseragent.com/";

		        #region Constructors

		        // The IntPtr and initWithCoder constructors are required for controllers that need 
		        // to be able to be created from a xib rather than from managed code
		        public UAFakeBrowerController (IntPtr handle) : base(handle)
		        {
			        Initialize ();
		        }

		        [Export("initWithCoder:")]
		        public UAFakeBrowerController (NSCoder coder) : base(coder)
		        {
			        Initialize ();
		        }

		        public UAFakeBrowerController () : base("UAFakeBrowerController", null)
		        {
			        Initialize ();
		        }

		        void Initialize ()
		        {
		        }
        		
		        #endregion


                #region Override methods
                public override void ViewDidLoad()
                {
                    base.ViewDidLoad();
                    _urlTextField.ShouldReturn = doReturn;                    
                    _browser.Delegate = new customWebViewDelegate(this);

                    
                    var webDocumentView = new NSObject(
                        Messaging.IntPtr_objc_msgSend(_browser.Handle, (new Selector("_documentView").Handle)));
                    var webView = webDocumentView.GetNativeField("_webView");
                    Messaging.void_objc_msgSend_IntPtr(webView.Handle, (new Selector("setCustomUserAgent:")).Handle,
                        (new NSString(@"Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0")).Handle);
                     /*
                      * 
                      * /* http://d.hatena.ne.jp/KishikawaKatsumi/20090217/1234818025
                      NSString *userAgent = 
                            @"Mozilla/5.0 (iPhone; U; CPU iPhone OS 2_1 like Mac OS X; ja-jp) AppleWebKit/525.18.1 (KHTML, like Gecko) Version/3.1.1 Mobile/5F136 Safari/525.20";

                        id webDocumentView;
                        id webView;
                        webDocumentView = objc_msgSend(myWebView, @selector(_documentView));
                        object_getInstanceVariable(webDocumentView, "_webView", (void**)&webView);
                        objc_msgSend(webView, @selector(setCustomUserAgent:), userAgent); */

                    loadURL(_startURL);
                }

                bool doReturn (UITextField tf)  
                {  
                    tf.ResignFirstResponder();  
                    loadURL(tf.Text);
                    return true;
                } 
                #endregion

                #region private methods
                void loadURL(string url)
                { 
                    var nsurl = new NSUrl(url);
                    var request = new NSMutableUrlRequest(nsurl);

                    /* 20100603 10:2 ÇæÇﬂÇ¡Ç∑
                    request.Headers = new NSDictionary();
                    request.Headers[new NSString("User-Agent")] = new NSString("SomeOne/1.0");
                    request.Headers[new NSString("User_Agent")] = new NSString("SomeOne/1.0");
                    */ 

                    /** 20100603 10:16 É_ÉÅÇ¡Ç∑
                    Selector selSetValueForHttpHeaderField = new Selector("setValue:forHTTPHeaderField:");
                    Messaging.void_objc_msgSend_IntPtr_IntPtr(request.Handle, selSetValueForHttpHeaderField.Handle,
                        new NSString("My-Special-Agent/1.0").Handle,
                        new NSString("User-Agent").Handle); 
                     */

                    /* 20100602 ÇæÇﬂÇ¡Ç∑
                    Selector selSetValueForHttpHeaderField = new Selector("setValue:forHTTPHeaderField:");
                    // Set the User_Agent HTTP Header
                    Messaging.void_objc_msgSend_IntPtr_IntPtr(request.Handle, selSetValueForHttpHeaderField.Handle,
                        new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0").Handle,
                        new NSString("User_Agent").Handle);

                    Messaging.void_objc_msgSend_IntPtr_IntPtr(request.Handle, selSetValueForHttpHeaderField.Handle,
                        new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0").Handle,
                        new NSString("User-Agent").Handle);

                    Messaging.void_objc_msgSend_IntPtr_IntPtr(request.Handle, selSetValueForHttpHeaderField.Handle,
                        new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0").Handle,
                        new NSString("userAgent").Handle);

                    Messaging.void_objc_msgSend_IntPtr_IntPtr(request.Handle, selSetValueForHttpHeaderField.Handle,
                        new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0").Handle,
                        new NSString("user_agent").Handle);

                    Messaging.void_objc_msgSend_IntPtr_IntPtr(request.Handle, selSetValueForHttpHeaderField.Handle,
                        new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0").Handle,
                        new NSString("user-agent").Handle);
                    */
                    //request["User_Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0";            
                    //request._SetValue("User_Agent", "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0");
                    //request.Headers = new NSDictionary();
                    //request.Headers[new NSString("User_Agent")] = new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0");            
                    _browser.LoadRequest(request);
                }
                #endregion

                #region Delegate
                internal class customWebViewDelegate : UIWebViewDelegate
                {
                    readonly UAFakeBrowerController _ctr;
                    public customWebViewDelegate(UAFakeBrowerController ctr) : base()
                    {
                        _ctr = ctr;
                    }
                    public override void LoadFailed(UIWebView webView, NSError error)
                    {
                        System.Diagnostics.Debug.WriteLine("Browser LoadFailed() : " + error.ToString());
                    }
                    public override void LoadingFinished(UIWebView webView)
                    {                            
                        _ctr._urlTextField.Text = webView.Request.Url.ToString();
                        System.Diagnostics.Debug.WriteLine("Browser LoadFinished() : " + webView.Request.Url.ToString());
                    }
                    public override void LoadStarted(UIWebView webView)
                    {
                        System.Diagnostics.Debug.WriteLine("Browser LoadStarted() : " + webView.Request.Url.ToString());
                    }
                    public override bool ShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
                    {
                        System.Diagnostics.Debug.WriteLine("Browser ShouldStartLoad() : " + request.Url.ToString());

                        /* 20100603 10:15 ÇæÇﬂÇ¡Ç∑
                        Selector selSetValueForHttpHeaderField = new Selector("setValue:forHTTPHeaderField:");
                        // Set the User_Agent HTTP Header
                        Messaging.void_objc_msgSend_IntPtr_IntPtr(request.Handle, selSetValueForHttpHeaderField.Handle,
                            new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0").Handle,
                            new NSString("User_Agent").Handle);
                        
                        Messaging.void_objc_msgSend_IntPtr_IntPtr(request.Handle, selSetValueForHttpHeaderField.Handle,
                            new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0").Handle,
                            new NSString("User-Agent").Handle);
                        */

                        // http://redth.info/2009/09/08/monotouch-https-post/
                        // http://stackoverflow.com/questions/478387/change-user-agent-in-uiwebview-iphone-sdk
                        /* does not work
                        Selector selSetValueForHttpHeaderField = new Selector("setValue:forHTTPHeaderField:");
                        // Set the User_Agent HTTP Header
                        Messaging.void_objc_msgSend_IntPtr_IntPtr(request.Handle, selSetValueForHttpHeaderField.Handle,
                            new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0").Handle,
                            new NSString("User_Agent").Handle);

                        Messaging.void_objc_msgSend_IntPtr_IntPtr(request.Handle, selSetValueForHttpHeaderField.Handle,    
                            new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0").Handle,    
                            new NSString("User-Agent").Handle);

                        Messaging.void_objc_msgSend_IntPtr_IntPtr(request.Handle, selSetValueForHttpHeaderField.Handle,
                            new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0").Handle,
                            new NSString("userAgent").Handle);
                        
                        Messaging.void_objc_msgSend_IntPtr_IntPtr(request.Handle, selSetValueForHttpHeaderField.Handle,
                            new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0").Handle,
                            new NSString("user_agent").Handle);
                        
                        Messaging.void_objc_msgSend_IntPtr_IntPtr(request.Handle, selSetValueForHttpHeaderField.Handle,
                            new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0").Handle,
                            new NSString("user-agent").Handle);
                        */
                        /*
                        //Instantiate a NSMutableURLRequest
                        IntPtr nsMutableRequestPtr = Messaging.IntPtr_objc_msgSend(new Class("NSMutableURLRequest").Handle, new Selector("new").Handle);

                        //Since NSMutableURLRequest subclasses NSUrlRequest, we can use NSURLRequest to work with
                        NSUrlRequest req = (NSUrlRequest)Runtime.GetNSObject(nsMutableRequestPtr);

                        //Set the url of the request
                        Messaging.void_objc_msgSend_IntPtr(req.Handle, new Selector("setURL:").Handle, new NSUrl(url).Handle);

                        //Set the HTTP Method (POST)
                        Messaging.void_objc_msgSend_IntPtr(req.Handle, new Selector("setHTTPMethod:").Handle, new NSString("POST").Handle);

                        //Make a selector to be used twice
                        Selector selSetValueForHttpHeaderField = new Selector("setValue:forHTTPHeaderField:");

                        //Set the Content-Length HTTP Header
                        Messaging.void_objc_msgSend_IntPtr_IntPtr(req.Handle,
                        selSetValueForHttpHeaderField.Handle,
                        new NSString(postData.Length.ToString()).Handle,
                        new NSString("Content-Length").Handle);

                        //Set the Content-Type HTTP Header
                        Messaging.void_objc_msgSend_IntPtr_IntPtr(req.Handle,
                        selSetValueForHttpHeaderField.Handle,
                        new NSString("application/x-www-form-urlencoded").Handle,
                        new NSString("Content-Type").Handle);                         */
                        //request.Headers[new NSString("User-Agent")] = new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0");
                        //request.Headers[new NSString("User_Agent")] = new NSString("Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/532.0 (KHTML, like Gecko) Chrome/3.0.195.38 Safari/532.0");

                        return true;
                    }
                }
                #endregion

            }
        }

