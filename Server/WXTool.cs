namespace Adolph.ThirdPartyTools
{
    /// <summary>
    /// 微信支付工具 基类
    /// </summary>
    public class WXPayTool
    {

        /// <summary>
        /// 获取微信签名
        /// </summary>
        /// <param name="sParams"></param>
        /// <param name="key">自己设置的证书密钥</param>
        /// <returns></returns>
        public static string GetSign(SortedDictionary<string, string> sParams, string key)
        {
            string sign = string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> temp in sParams)
            {
                if (temp.Value == "" || temp.Value == null || temp.Key.ToLower() == "sign")
                {
                    continue;
                }
                sb.Append(temp.Key.Trim() + "=" + temp.Value.Trim() + "&");
            }
            sb.Append("key=" + key.Trim());
            string signkey = sb.ToString();
            sign = WXPayTool.GetMD5(signkey, "utf-8");

            return sign;
        }

        /// <summary>
        /// 获取大写的MD5签名结果 
        /// </summary>
        /// <param name="encypStr"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        public static string GetMD5(string encypStr, string charset = "UTF-8")
        {

            string retStr = "";
            MD5CryptoServiceProvider m5 = new MD5CryptoServiceProvider();

            //创建md5对象
            byte[] inputBye;
            byte[] outputBye;

            //使用GB2312编码方式把字符串转化为字节数组．
            try
            {
                inputBye = Encoding.GetEncoding(charset).GetBytes(encypStr);
            }
            catch 
            {
                inputBye = Encoding.GetEncoding("GB2312").GetBytes(encypStr);
            }
            outputBye = m5.ComputeHash(inputBye);
            //for (int i = 0; i < outputBye.Length; i++)
            //{
            //    // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 
            //    retStr = retStr + outputBye[i].ToString("X");
            //}
            retStr = System.BitConverter.ToString(outputBye);
            retStr = retStr.Replace("-", "").ToUpper();
            return retStr;
        }

        /// <summary>
        /// 统一支付接口
        /// </summary>
        public const string UnifiedPayUrl = "https://api.mch.weixin.qq.com/pay/unifiedorder";

        /// <summary>
        /// 网页授权接口
        /// </summary>
        public const string access_tokenUrl = "https://api.weixin.qq.com/sns/oauth2/access_token";

        /// <summary>
        /// 微信订单查询接口
        /// </summary>
        public const string OrderQueryUrl = "https://api.mch.weixin.qq.com/pay/orderquery";

        /// <summary>
        /// 随机串
        /// </summary>
        public static string getNoncestr()
        {
            Random random = new Random();
            return WXPayTool.GetMD5(random.Next(1000).ToString(), "UTF-8").ToLower().Replace("s", "S");
        }

        /// <summary>
        /// 时间截，自1970年以来的秒数
        /// </summary>
        public static int getTimestamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt32(ts.TotalSeconds);
        }
        /// <summary>
        /// 网页授权接口
        /// </summary>
        public static string getAccess_tokenUrl()
        {
            return access_tokenUrl;
        }


        /// <summary>
        /// post数据到指定接口并返回数据
        /// </summary>
        public static string PostXmlToUrl(string url, string postData)
        {
            string returnmsg = "";
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                returnmsg = wc.UploadString(url, "POST", postData);
            }
            //returnmsg = Encoding.UTF8.GetString(Encoding.GetEncoding("GBK").GetBytes(returnmsg));
            return returnmsg;
        }

        /// <summary>
        /// 获取prepay_id
        /// </summary>
        public string getPrepay_id(UnifiedOrder order, string key)
        {
            string prepay_id = "";
            string post_data = getUnifiedOrderXml(order, key);
            string request_data = PostXmlToUrl(UnifiedPayUrl, post_data);
            SortedDictionary<string, string> requestXML = GetInfoFromXml(request_data);
            foreach (KeyValuePair<string, string> k in requestXML)
            {
                if (k.Key == "prepay_id")
                {
                    prepay_id = k.Value;
                    break;
                }
            }
            return prepay_id;
        }

        /// <summary>
        /// 获取prepay_id
        /// </summary>
        public string getPrepay_id(UnifiedOrder order, string key, out string return_msg)
        {
            return_msg = "";
            string prepay_id = "";
            string post_data = getUnifiedOrderXml(order, key);
            string request_data = PostXmlToUrl(UnifiedPayUrl, post_data);
            return_msg = request_data;
            SortedDictionary<string, string> requestXML = GetInfoFromXml(request_data);
            foreach (KeyValuePair<string, string> k in requestXML)
            {
                if (k.Key == "prepay_id")
                {
                    prepay_id = k.Value;
                    break;
                }
            }
            return prepay_id;
        }

        /// <summary>
        /// 获取微信订单明细
        /// </summary>
        public static OrderDetail getOrderDetail(QueryOrder queryorder, string key)
        {
            string post_data = getQueryOrderXml(queryorder, key);
            string request_data = PostXmlToUrl(OrderQueryUrl, post_data);
            return GetOrderDetialFromXML(request_data);
        }

        /// <summary>
        /// 获取微信订单明细(from xml)
        /// </summary>
        public static OrderDetail GetOrderDetialFromXML(string xmlstring)
        {
            return GetOrderDetialFromDictionary(GetInfoFromXml(xmlstring));
        }

        /// <summary>
        /// 获取微信订单明细(from Dictionary)
        /// </summary>
        public static OrderDetail GetOrderDetialFromDictionary(SortedDictionary<string, string> OrderDetailData)
        {
            OrderDetail orderdetail = new OrderDetail();
            foreach (KeyValuePair<string, string> k in OrderDetailData)
            {
                switch (k.Key)
                {
                    case "return_code":
                        orderdetail.return_code = k.Value;
                        break;
                    case "return_msg":
                        orderdetail.return_msg = k.Value;
                        break;
                    case "appid":
                        orderdetail.appid = k.Value;
                        break;
                    case "mch_id":
                        orderdetail.mch_id = k.Value;
                        break;
                    case "nonce_str":
                        orderdetail.nonce_str = k.Value;
                        break;
                    case "sign":
                        orderdetail.sign = k.Value;
                        break;
                    case "result_code":
                        orderdetail.result_code = k.Value;
                        break;
                    case "err_code":
                        orderdetail.err_code = k.Value;
                        break;
                    case "err_code_des":
                        orderdetail.err_code_des = k.Value;
                        break;
                    case "trade_state":
                        orderdetail.trade_state = k.Value;
                        break;
                    case "device_info":
                        orderdetail.device_info = k.Value;
                        break;
                    case "openid":
                        orderdetail.openid = k.Value;
                        break;
                    case "is_subscribe":
                        orderdetail.is_subscribe = k.Value;
                        break;
                    case "trade_type":
                        orderdetail.trade_type = k.Value;
                        break;
                    case "bank_type":
                        orderdetail.bank_type = k.Value;
                        break;
                    case "cash_fee":
                        orderdetail.cash_fee = k.Value;
                        break;
                    case "total_fee":
                        orderdetail.total_fee = k.Value;
                        break;
                    case "coupon_fee":
                        orderdetail.coupon_fee = k.Value;
                        break;
                    case "fee_type":
                        orderdetail.fee_type = k.Value;
                        break;
                    case "transaction_id":
                        orderdetail.transaction_id = k.Value;
                        break;
                    case "out_trade_no":
                        orderdetail.out_trade_no = k.Value;
                        break;
                    case "attach":
                        orderdetail.attach = k.Value;
                        break;
                    case "time_end":
                        orderdetail.time_end = k.Value;
                        break;
                    default:
                        break;
                }
            }
            return orderdetail;
        }

        /// <summary>
        /// 把XML数据转换为SortedDictionary集合
        /// </summary>
        /// <param name="xmlstring">XML数据</param>
        /// <returns></returns>
        public static SortedDictionary<string, string> GetInfoFromXml(string xmlstring)
        {
            SortedDictionary<string, string> sParams = new SortedDictionary<string, string>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlstring);
                XmlElement root = doc.DocumentElement;
                int len = root.ChildNodes.Count;
                for (int i = 0; i < len; i++)
                {
                    string name = root.ChildNodes[i].Name;
                    if (!sParams.ContainsKey(name))
                    {
                        sParams.Add(name.Trim(), root.ChildNodes[i].InnerText.Trim());
                    }
                }
            }
            catch { }
            return sParams;
        }

        /// <summary>
        /// 微信统一下单接口xml参数整理
        /// </summary>
        /// <param name="order">微信支付参数实例</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string getUnifiedOrderXml(UnifiedOrder order, string key)
        {
            string return_string = string.Empty;
            SortedDictionary<string, string> sParams = new SortedDictionary<string, string>();
            sParams.Add("appid", order.appid);
            sParams.Add("attach", order.attach);
            sParams.Add("body", order.body);
            if (order.device_info != "")
                sParams.Add("device_info", order.device_info);
            sParams.Add("mch_id", order.mch_id);
            sParams.Add("nonce_str", order.nonce_str);
            sParams.Add("notify_url", order.notify_url);
            if (order.openid != "")
                sParams.Add("openid", order.openid);
            sParams.Add("out_trade_no", order.out_trade_no);
            sParams.Add("spbill_create_ip", order.spbill_create_ip);
            sParams.Add("total_fee", order.total_fee.ToString());
            sParams.Add("trade_type", order.trade_type);

            order.sign = GetSign(sParams, key);
            sParams.Add("sign", order.sign);

            //拼接成XML请求数据
            StringBuilder sbPay = new StringBuilder();
            foreach (KeyValuePair<string, string> k in sParams)
            {
                if (k.Key == "sign") continue;
                //if (k.Key == "attach" || k.Key == "body" || k.Key == "sign")
                //{
                //    sbPay.Append("<" + k.Key + "><![CDATA[" + k.Value + "]]></" + k.Key + ">");
                //}
                //else
                //{
                sbPay.Append("<" + k.Key + ">" + k.Value + "</" + k.Key + ">");
                //}
                sbPay.Append("\n");
            }
            //sbPay.Append("<sign><![CDATA[" + sParams["sign"] + "]]></sign>");
            sbPay.Append("<sign>" + sParams["sign"] + "</sign>");

            return_string = string.Format("<xml>{0}</xml>", sbPay.ToString());
            //byte[] byteArray = Encoding.UTF8.GetBytes(return_string);
            //return_string = Encoding.GetEncoding("GBK").GetString(Encoding.UTF8.GetBytes(return_string));
            return return_string;
        }


        /// <summary>
        /// 微信订单查询接口XML参数整理
        /// </summary>
        /// <param name="queryorder">微信订单查询参数实例</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string getQueryOrderXml(QueryOrder queryorder, string key)
        {
            string return_string = string.Empty;
            SortedDictionary<string, string> sParams = new SortedDictionary<string, string>();
            sParams.Add("appid", queryorder.appid);
            sParams.Add("mch_id", queryorder.mch_id);
            sParams.Add("transaction_id", queryorder.transaction_id);
            sParams.Add("out_trade_no", queryorder.out_trade_no);
            sParams.Add("nonce_str", queryorder.nonce_str);
            queryorder.sign = GetSign(sParams, key);
            sParams.Add("sign", queryorder.sign);

            //拼接成XML请求数据
            StringBuilder sbPay = new StringBuilder();
            foreach (KeyValuePair<string, string> k in sParams)
            {
                if (k.Key == "attach" || k.Key == "body" || k.Key == "sign")
                {
                    sbPay.Append("<" + k.Key + "><![CDATA[" + k.Value + "]]></" + k.Key + ">");
                }
                else
                {
                    sbPay.Append("<" + k.Key + ">" + k.Value + "</" + k.Key + ">");
                }
            }
            return_string = string.Format("<xml>{0}</xml>", sbPay.ToString().TrimEnd(','));
            return return_string;
        }


        /// <summary>
        /// 获取微信订单详情XML
        /// </summary>
        /// <param name="orderDetial">微信订单详情</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string getOrderDetailXml(OrderDetail orderDetial, string key)
        {
            string return_string = string.Empty;
            SortedDictionary<string, string> sParams = new SortedDictionary<string, string>();
            sParams.Add("appid", orderDetial.appid);
            sParams.Add("mch_id", orderDetial.mch_id);
            sParams.Add("transaction_id", orderDetial.transaction_id);
            sParams.Add("out_trade_no", orderDetial.out_trade_no);
            sParams.Add("nonce_str", orderDetial.nonce_str);
            orderDetial.sign = GetSign(sParams, key);
            sParams.Add("sign", orderDetial.sign);

            //拼接成XML请求数据
            StringBuilder sbPay = new StringBuilder();
            foreach (KeyValuePair<string, string> k in sParams)
            {
                if (k.Key == "attach" || k.Key == "body" || k.Key == "sign")
                {
                    sbPay.Append("<" + k.Key + "><![CDATA[" + k.Value + "]]></" + k.Key + ">");
                }
                else
                {
                    sbPay.Append("<" + k.Key + ">" + k.Value + "</" + k.Key + ">");
                }
            }
            return_string = string.Format("<xml>{0}</xml>", sbPay.ToString().TrimEnd(','));
            return return_string;
        }

        /// <summary>
        /// 获取微信订单详情XML
        /// </summary>
        /// <param name="orderDetial">微信订单详情</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string getOrderDetailXml1(OrderDetail orderDetial, string key)
        {
            string return_string = string.Empty;
            SortedDictionary<string, string> sParams = new SortedDictionary<string, string>();
            sParams.Add("appid", orderDetial.appid);
            sParams.Add("return_code", orderDetial.return_code);
            sParams.Add("return_msg", orderDetial.return_msg);
            sParams.Add("attach", orderDetial.attach);
            sParams.Add("result_code", orderDetial.result_code);
            sParams.Add("openid", orderDetial.openid);
            sParams.Add("is_subscribe", orderDetial.is_subscribe);
            sParams.Add("trade_type", orderDetial.trade_type);
            sParams.Add("bank_type", orderDetial.bank_type);
            sParams.Add("total_fee", orderDetial.total_fee);
            sParams.Add("fee_type", orderDetial.fee_type);
            sParams.Add("time_end", orderDetial.time_end);
            sParams.Add("trade_state", orderDetial.trade_state);
            sParams.Add("cash_fee", orderDetial.cash_fee);

            sParams.Add("mch_id", orderDetial.mch_id);
            sParams.Add("transaction_id", orderDetial.transaction_id);
            sParams.Add("out_trade_no", orderDetial.out_trade_no);
            sParams.Add("nonce_str", orderDetial.nonce_str);
            orderDetial.sign = GetSign(sParams, key);
            sParams.Add("sign", orderDetial.sign);

            //拼接成XML请求数据
            StringBuilder sbPay = new StringBuilder();
            foreach (KeyValuePair<string, string> k in sParams)
            {
                if (k.Key == "attach" || k.Key == "body" || k.Key == "sign")
                {
                    sbPay.Append("<" + k.Key + "><![CDATA[" + k.Value + "]]></" + k.Key + ">");
                }
                else
                {
                    sbPay.Append("<" + k.Key + ">" + k.Value + "</" + k.Key + ">");
                }
            }
            return_string = string.Format("<xml>{0}</xml>", sbPay.ToString().TrimEnd(','));
            return return_string;
        }

    }

    /// <summary>
    /// 微信配置
    /// </summary>
    public class WXConfig
    {
        #region APP支付配置

        /// <summary>
        /// APP支付的APPID
        /// </summary>
        public static string APPPAY_APPID
        {
            get
            {
                try { return ConfigurationManager.AppSettings["apppay_appid"].ToString(); }
                catch { return ""; }
            }
        }

        /// <summary>
        /// APP支付的APPID集合
        /// </summary>
        public static LitJson.JsonData APPPAY_APPIDS
        {
            get
            {
                try
                {
                    return LitJson.JsonMapper.ToObject(ConfigurationManager.AppSettings["apppay_appids"].ToString());
                }
                catch { return null; }
            }
        }
        /// <summary>
        /// APP支付的mch_id
        /// </summary>
        public static string APPPAY_MCH_ID
        {
            get
            {
                try { return ConfigurationManager.AppSettings["apppay_mch_id"].ToString(); }
                catch { return ""; }
            }
        }
        /// <summary>
        /// APP支付的mch_id集合
        /// </summary>
        public static LitJson.JsonData APPPAY_MCH_IDS
        {
            get
            {
                try
                {
                    return LitJson.JsonMapper.ToObject(ConfigurationManager.AppSettings["apppay_mch_ids"].ToString());
                }
                catch { return null; }
            }
        }
        /// <summary>
        /// APP支付的notify_url(微信)
        /// </summary>
        public static string APPPAY_NOTIFY_URL
        {
            get
            {
                try { return ConfigurationManager.AppSettings["apppay_notify_url"].ToString(); }
                catch { return ""; }
            }
        }

        /// <summary>
        /// APP余额充值的notify_url(微信)
        /// </summary>
        public static string APPPAY_NOTIFY_URL_RECHARGE
        {
            get
            {
                try { return ConfigurationManager.AppSettings["apppay_notify_url_recharge"].ToString(); }
                catch { return ""; }
            }
        }
        #endregion

        /// <summary>
        /// ssl证书 密钥
        /// </summary>
        public static string SSL_CERT_KEY
        {
            get
            {
                try { return ConfigurationManager.AppSettings["ssl_cert_key"].ToString(); }
                catch { return ""; }
            }
        }

        /// <summary>
        /// API安全密钥
        /// </summary>
        public static string WX_API_KEY
        {
            get
            {
                try { return ConfigurationManager.AppSettings["wx_api_key"].ToString(); }
                catch { return ""; }
            }
        }
        /// <summary>
        /// API安全密钥
        /// </summary>
        public static LitJson.JsonData WX_API_KEYS
        {
            get
            {
                try
                {
                    return LitJson.JsonMapper.ToObject(ConfigurationManager.AppSettings["wx_api_keys"].ToString());
                }
                catch { return null; }
            }
        }
    }

    namespace Model
    {
        /// <summary>
        /// 微信统一接口请求实体对象
        /// </summary>
        [Serializable]
        public class UnifiedOrder
        {
            /// <summary>
            /// 公共号ID(微信分配的公众账号 ID)
            /// </summary>
            public string appid = "";
            /// <summary>
            /// 商户号(微信支付分配的商户号)
            /// </summary>
            public string mch_id = "";
            /// <summary>
            /// 微信支付分配的终端设备号
            /// </summary>
            public string device_info = "";
            /// <summary>
            /// 随机字符串，不长于 32 位
            /// </summary>
            public string nonce_str = "";
            /// <summary>
            /// 签名
            /// </summary>
            public string sign = "";
            /// <summary>
            /// 商品描述
            /// </summary>
            public string body = "";
            /// <summary>
            /// 附加数据，原样返回
            /// </summary>
            public string attach = "";
            /// <summary>
            /// 商户系统内部的订单号,32个字符内、可包含字母,确保在商户系统唯一,详细说明
            /// </summary>
            public string out_trade_no = "";
            /// <summary>
            /// 订单总金额，单位为分，不能带小数点
            /// </summary>
            public int total_fee = 0;
            /// <summary>
            /// 终端IP
            /// </summary>
            public string spbill_create_ip = "";
            /// <summary>
            /// 订 单 生 成 时 间 ， 格 式 为yyyyMMddHHmmss，如 2009 年12 月 25 日 9 点 10 分 10 秒表示为 20091225091010。时区为 GMT+8 beijing。该时间取自商户服务器
            /// </summary>
            public string time_start = "";
            /// <summary>
            /// 交易结束时间
            /// </summary>
            public string time_expire = "";
            /// <summary>
            /// 商品标记 商品标记，该字段不能随便填，不使用请填空，使用说明详见第 5 节
            /// </summary>
            public string goods_tag = "";
            /// <summary>
            /// 接收微信支付成功通知
            /// </summary>
            public string notify_url = "";
            /// <summary>
            /// JSAPI、NATIVE、APP
            /// </summary>
            public string trade_type = "";
            /// <summary>
            /// 用户标识 trade_type 为 JSAPI时，此参数必传
            /// </summary>
            public string openid = "";
            /// <summary>
            /// 只在 trade_type 为 NATIVE时需要填写。
            /// </summary>
            public string product_id = "";
        }

        /// <summary>
        /// 微信订单查询接口请求实体对象
        /// </summary>
        [Serializable]
        public class QueryOrder
        {
            /// <summary>
            /// 公共号ID(微信分配的公众账号 ID)
            /// </summary>
            public string appid = "";

            /// <summary>
            /// 商户号(微信支付分配的商户号)
            /// </summary>
            public string mch_id = "";

            /// <summary>
            /// 微信订单号，优先使用
            /// </summary>
            public string transaction_id = "";

            /// <summary>
            /// 商户系统内部订单号
            /// </summary>
            public string out_trade_no = "";

            /// <summary>
            /// 随机字符串，不长于 32 位
            /// </summary>
            public string nonce_str = "";

            /// <summary>
            /// 签名，参与签名参数：appid，mch_id，transaction_id，out_trade_no，nonce_str，key
            /// </summary>
            public string sign = "";

            /// <summary>
            /// 获取签名值
            /// </summary>
            /// <param name="key">自己设置的证书密钥</param>
            /// <returns></returns>
            public string GetSign(string key)
            {
                SortedDictionary<string, string> sParams = new SortedDictionary<string, string>();
                sParams.Add("appid", this.appid);
                sParams.Add("mch_id", this.mch_id);
                sParams.Add("transaction_id", this.transaction_id);
                sParams.Add("out_trade_no", this.out_trade_no);
                sParams.Add("nonce_str", this.nonce_str);
                return WXPayTool.GetSign(sParams, key);
            }
        }

        /// <summary>
        /// 微信预支付订单
        /// </summary>
        [Serializable]
        public class PrepayOrder
        {
            /// <summary>
            /// 应用ID,微信开放平台审核通过的应用APPID
            /// </summary>
            public string appid = "";
            /// <summary>
            /// 商户号,微信支付分配的商户号
            /// </summary>
            public string partnerid = "";
            /// <summary>
            /// 预支付交易会话ID,微信返回的支付交易会话ID
            /// </summary>
            public string prepayid = "";
            /// <summary>
            /// 扩展字段,暂填写固定值Sign=WXPay
            /// </summary>
            public string package = "";
            /// <summary>
            /// 随机字符串，不长于32位
            /// </summary>
            public string noncestr = "";
            /// <summary>
            /// 时间戳
            /// </summary>
            public int timestamp = 0;
            /// <summary>
            /// 签名
            /// </summary>
            public string sign = "";
            /// <summary>
            /// 获取签名值
            /// </summary>
            /// <param name="key">自己设置的证书密钥</param>
            /// <returns></returns>
            public string GetSign(string key)
            {
                SortedDictionary<string, string> sParams = new SortedDictionary<string, string>();
                sParams.Add("appid", this.appid);
                sParams.Add("partnerid", this.partnerid);
                sParams.Add("prepayid", this.prepayid);
                sParams.Add("noncestr", this.noncestr);
                sParams.Add("package", this.package);
                sParams.Add("timestamp", this.timestamp.ToString());
                return WXPayTool.GetSign(sParams, key);
            }
        }

        /// <summary>
        /// 微信订单明细实体对象
        /// </summary>
        [Serializable]
        public class OrderDetail
        {
            /// <summary>
            /// 返回状态码，SUCCESS/FAIL 此字段是通信标识，非交易标识，交易是否成功需要查看trade_state来判断
            /// </summary>
            public string return_code = "";

            /// <summary>
            /// 返回信息返回信息，如非空，为错误原因 签名失败 参数格式校验错误
            /// </summary>
            public string return_msg = "";

            /// <summary>
            /// 公共号ID(微信分配的公众账号 ID)
            /// </summary>
            public string appid = "";

            /// <summary>
            /// 商户号(微信支付分配的商户号)
            /// </summary>
            public string mch_id = "";

            /// <summary>
            /// 随机字符串，不长于32位
            /// </summary>
            public string nonce_str = "";

            /// <summary>
            /// 签名
            /// </summary>
            public string sign = "";

            /// <summary>
            /// 业务结果,SUCCESS/FAIL
            /// </summary>
            public string result_code = "";

            /// <summary>
            /// 错误代码
            /// </summary>
            public string err_code = "";

            /// <summary>
            /// 错误代码描述
            /// </summary>
            public string err_code_des = "";

            /// <summary>
            /// 交易状态
            ///SUCCESS—支付成功
            ///REFUND—转入退款
            ///NOTPAY—未支付
            ///CLOSED—已关闭
            ///REVOKED—已撤销
            ///USERPAYING--用户支付中
            ///NOPAY--未支付(输入密码或确认支付超时) PAYERROR--支付失败(其他原因，如银行返回失败)
            /// </summary>
            public string trade_state = "";

            /// <summary>
            /// 微信支付分配的终端设备号
            /// </summary>
            public string device_info = "";

            /// <summary>
            /// 用户在商户appid下的唯一标识
            /// </summary>
            public string openid = "";

            /// <summary>
            /// 用户是否关注公众账号，Y-关注，N-未关注，仅在公众账号类型支付有效
            /// </summary>
            public string is_subscribe = "";

            /// <summary>
            /// 交易类型,JSAPI、NATIVE、MICROPAY、APP
            /// </summary>
            public string trade_type = "";

            /// <summary>
            /// 银行类型，采用字符串类型的银行标识
            /// </summary>
            public string bank_type = "";

            /// <summary>
            /// 现金支付金额
            /// </summary>
            public string cash_fee = "";

            /// <summary>
            /// 订单总金额，单位为分
            /// </summary>
            public string total_fee = "";

            /// <summary>
            /// 现金券支付金额 小于订单总金额，订单总金额-现金券金额为现金支付金额
            /// </summary>
            public string coupon_fee = "";

            /// <summary>
            /// 货币类型，符合ISO 4217标准的三位字母代码，默认人民币：CNY
            /// </summary>
            public string fee_type = "";

            /// <summary>
            /// 微信支付订单号
            /// </summary>
            public string transaction_id = "";

            /// <summary>
            /// 商户系统的订单号，与请求一致。
            /// </summary>
            public string out_trade_no = "";

            /// <summary>
            /// 商家数据包，原样返回
            /// </summary>
            public string attach = "";

            /// <summary>
            /// 支付完成时间，格式为yyyyMMddhhmmss，如2009年12月27日9点10分10秒表示为20091227091010。
            /// 时区为GMT+8 beijing。该时间取自微信支付服务器
            /// </summary>
            public string time_end = "";

            /// <summary>
            /// 获取签名值
            /// </summary>
            /// <param name="key">自己设置的证书密钥</param>
            /// <returns></returns>
            public string GetSign(string key)
            {
                SortedDictionary<string, string> sParams = new SortedDictionary<string, string>();
                sParams.Add("appid", this.appid);
                sParams.Add("return_code", this.return_code);
                sParams.Add("return_msg", this.return_msg);
                sParams.Add("mch_id", this.mch_id);
                sParams.Add("nonce_str", this.nonce_str);
                sParams.Add("result_code", this.result_code);
                sParams.Add("err_code", this.err_code);
                sParams.Add("err_code_des", this.err_code_des);
                sParams.Add("trade_state", this.trade_state);
                sParams.Add("device_info", this.device_info);
                sParams.Add("openid", this.openid);
                sParams.Add("is_subscribe", this.is_subscribe);
                sParams.Add("trade_type", this.trade_type);
                sParams.Add("bank_type", this.bank_type);
                sParams.Add("cash_fee", this.cash_fee);
                sParams.Add("total_fee", this.total_fee);
                sParams.Add("coupon_fee", this.coupon_fee);
                sParams.Add("fee_type", this.fee_type);
                sParams.Add("transaction_id", this.transaction_id);
                sParams.Add("out_trade_no", this.out_trade_no);
                sParams.Add("attach", this.attach);
                sParams.Add("time_end", this.time_end);

                return WXPayTool.GetSign(sParams, key);
            }


        }
    }

}
