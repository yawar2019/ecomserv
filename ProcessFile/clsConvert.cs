using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
public class clsConvert
{
    public clsConvert()
    {

    }
    private string ser = "10165224392";
    public SautinSoft.PdfFocus get_obj()
    {
        try
        {
            SautinSoft.PdfFocus obj = new SautinSoft.PdfFocus();
            obj.Serial = ser;
            return obj;
        }
        catch(Exception exp)
        {
            log("get_obj", exp.Message);
        }
        return null;
    }
    public void kill_obj(SautinSoft.PdfFocus obj)
    {
        try
        {
            obj.ClosePdf();
            obj = null;
        }
        catch (Exception exp)
        {

        }
    }
    public void log(string name, string desc)
    {
        try
        {

        }
        catch
        {

        }
    }
}