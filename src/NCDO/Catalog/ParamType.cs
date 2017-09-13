using System;
using System.Collections.Generic;
using System.Text;

namespace NCDO.Catalog
{
    public enum ParamType
    {
        Path,
        Query,
        RequestBody,
        ResponseBody,
        RequestResponseBody,
        Matrix,
        Form,
        Cookie,
        Header
    }
}
