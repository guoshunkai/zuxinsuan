using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenel
{
    public class HttpResult
    {

        private string _ErrorMessage;

        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; }
        }

        private string _KeyValue;

        public string KeyValue
        {
            get { return _KeyValue; }
            set { _KeyValue = value; }
        }

        //默认值为true
        private bool _Result = true;

        public bool Result
        {
            get { return _Result; }
            set { _Result = value; }
        }

        public string ToJson()
        {
            return JsonHelper.Encode(this);
        }
    }
}
