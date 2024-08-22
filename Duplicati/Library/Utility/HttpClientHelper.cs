// Copyright (C) 2024, The Duplicati Team
// https://duplicati.com, hello@duplicati.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

using System.Net.Http;

namespace Duplicati.Library.Utility;

/// <summary>
/// This class was created as a proxy to access the HttpClientFactory
/// in places where dependency injection is not yet implemented or
/// desirable.
/// </summary>
public static class HttpClientHelper
{

    /// <summary>
    /// Central HttpClient singleton instance to be used across the application as 
    /// per the HttpClientFactory recommended pattern.
    /// </summary>
    public static HttpClient DefaultClient { get; private set; }
    
    /// <summary>
    /// Sets the factory reference.
    /// </summary>
    /// <param name="factory">IHttpClientFactory instance</param>
    public static void Configure(IHttpClientFactory factory)
    {
        DefaultClient = factory.CreateClient();
    }
}