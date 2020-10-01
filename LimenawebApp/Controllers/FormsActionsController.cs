﻿using LimenawebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace LimenawebApp.Controllers
{
    public class FormsActionsController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private dbComerciaEntities dbcmk = new dbComerciaEntities();
        private DLI_PROEntities dlipro = new DLI_PROEntities();

        public class MyObj_formtemplate
        {
            public string id { get; set; }
            public string text { get; set; }
            public string value { get; set; }
        }
        public class subcategories
        {
            public string FirmCode { get; set; }
            public string FirmName { get; set; }
            public string Category { get; set; }
            public Boolean isselected { get; set; }
        }

        public class categories
        {
            public string FirmCode { get; set; }
            public string FirmName { get; set; }
            public string Customer { get; set; }
            public Boolean isselected { get; set; }
        }

        public class brands
        {
            public string FirmCode { get; set; }
            public string FirmName { get; set; }
            public string Customer { get; set; }
            public Boolean isselected { get; set; }
        }
        public class productline
        {
            public string Id_subcategory { get; set; }
            public string SubCategory { get; set; }
            public string Brand { get; set; }
            public Boolean isselected { get; set; }
        }

        public class brandcompetitor
        {
            public string Id_brandc { get; set; }
            public string namec { get; set; }
            public string Brand { get; set; }
            public Boolean isselected { get; set; }
        }


                public JsonResult Finish_activity(string id, string lat, string lng, string check_out)
        {
            try
            {
                int IDU = Convert.ToInt32(Session["IDusuario"]);
                if (id != null)
                {
                    int act = Convert.ToInt32(id);
                    ActivitiesM activity = dbcmk.ActivitiesM.Find(act);

                    //if (lat != null || lat != "")
                    //{
                    //    //Guardamos el log de la actividad
                    //    ActivitiesM_log nuevoLog = new ActivitiesM_log();
                    //    nuevoLog.latitude = lat;
                    //    nuevoLog.longitude = lng;
                    //    nuevoLog.ID_usuario = IDU;
                    //    nuevoLog.ID_activity = Convert.ToInt32(id);
                    //    nuevoLog.fecha_conexion = Convert.ToDateTime(check_out);
                    //    nuevoLog.query1 = "";
                    //    nuevoLog.query2 = "";
                    //    nuevoLog.action = "FINISH ACTIVITY - " + activity.description;
                    //    nuevoLog.ip = "";
                    //    nuevoLog.hostname = "";
                    //    nuevoLog.typeh = "";
                    //    nuevoLog.continent_name = "";
                    //    nuevoLog.country_code = "";
                    //    nuevoLog.country_name = "";
                    //    nuevoLog.region_code = "";
                    //    nuevoLog.region_name = "";
                    //    nuevoLog.city = "";

                    //    dbcmk.ActivitiesM_log.Add(nuevoLog);
                    //    dbcmk.SaveChanges();
                    //}

                    activity.check_out = Convert.ToDateTime(check_out);
                    activity.isfinished = true;
                    dbcmk.Entry(activity).State = EntityState.Modified;
                    dbcmk.SaveChanges();
                    

                    return Json(new { Result = "Success" });
                }
                return Json(new { Result = "Warning" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "Warning" + ex.Message });
            }
        }
        public JsonResult Finish_activitySurvey(string id, string lat, string lng, string check_out)
        {
            try
            {
                int IDU = Convert.ToInt32(Session["IDusuario"]);
                if (id != null)
                {
                    int act = Convert.ToInt32(id);
                    Tasks activity = dbcmk.Tasks.Find(act);

                    //if (lat != null || lat != "")
                    //{
                    //    //Guardamos el log de la actividad
                    //    ActivitiesM_log nuevoLog = new ActivitiesM_log();
                    //    nuevoLog.latitude = lat;
                    //    nuevoLog.longitude = lng;
                    //    nuevoLog.ID_usuario = IDU;
                    //    nuevoLog.ID_activity = Convert.ToInt32(id);
                    //    nuevoLog.fecha_conexion = Convert.ToDateTime(check_out);
                    //    nuevoLog.query1 = "";
                    //    nuevoLog.query2 = "";
                    //    nuevoLog.action = "FINISH ACTIVITY - " + activity.description;
                    //    nuevoLog.ip = "";
                    //    nuevoLog.hostname = "";
                    //    nuevoLog.typeh = "";
                    //    nuevoLog.continent_name = "";
                    //    nuevoLog.country_code = "";
                    //    nuevoLog.country_name = "";
                    //    nuevoLog.region_code = "";
                    //    nuevoLog.region_name = "";
                    //    nuevoLog.city = "";

                    //    dbcmk.ActivitiesM_log.Add(nuevoLog);
                    //    dbcmk.SaveChanges();
                    //}

                    activity.end_date = Convert.ToDateTime(check_out);
                    activity.ID_taskstatus = 4;
                    dbcmk.Entry(activity).State = EntityState.Modified;
                    dbcmk.SaveChanges();
                    

                    return Json(new { Result = "Success" });
                }
                return Json(new { Result = "Warning" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "Warning" + ex.Message });
            }
        }

        public ActionResult GetCategories(string customerID, string idvisit)
        {
            try
            {
                if (customerID != null)
                {
                    int bra = Convert.ToInt32(customerID);
                    try
                    {
                        int IDV = Convert.ToInt32(idvisit);
                        var lstcat = dlipro.BI_Dim_Products
                                .Where(i => i.id_brand == bra)
                                .Select(i => new categories { FirmCode = i.id_category.ToString(), FirmName = i.category_name, isselected = false, Customer = "" })
                                .Distinct()
                                .OrderByDescending(i => i.FirmName)
                                .ToList();

                        var itemselectcat = (from br in dbcmk.FormsM_details where (br.ID_formresourcetype == 30 && br.ID_visit == IDV) select br).FirstOrDefault();
                        if (itemselectcat != null)
                        {
                            foreach (var item in lstcat)
                            {
                                if (item.FirmCode.ToString() == itemselectcat.fvalueText)
                                {
                                    item.isselected = true;
                                }

                            }
                        }


                        //}
                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        string result = javaScriptSerializer.Serialize(lstcat);
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    catch {
                 
                        var lstcat = dlipro.BI_Dim_Products
                         .Where(i => i.id_brand == bra)
                                .Select(i => new categories { FirmCode = i.id_category.ToString(), FirmName = i.category_name, isselected = false, Customer = "" })
                                .Distinct()
                                .OrderByDescending(i => i.FirmName)
                                .ToList();
                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        string result = javaScriptSerializer.Serialize(lstcat);
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
          
                }
            }
            catch
            {
                int IDV = Convert.ToInt32(idvisit);
                var lstcat = dlipro.BI_Dim_Products
                        .Where(i => i.id_Vendor == customerID)
                        .Select(i => new categories { FirmCode = i.id_category.ToString(), FirmName = i.category_name, isselected = false, Customer = "" })
                        .Distinct()
                        .OrderByDescending(i => i.FirmName)
                        .ToList();

                var itemselectcat = (from br in dbcmk.FormsM_details where (br.ID_formresourcetype == 30 && br.ID_visit == IDV) select br).FirstOrDefault();
                if (itemselectcat != null)
                {
                    foreach (var item in lstcat)
                    {
                        if (item.FirmCode.ToString() == itemselectcat.fvalueText)
                        {
                            item.isselected = true;
                        }

                    }
                }


                //}
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result = javaScriptSerializer.Serialize(lstcat);
                return Json(result, JsonRequestBehavior.AllowGet);


          
            }
            return Json("error", JsonRequestBehavior.AllowGet);

        }

        public ActionResult getSubcategories(string customerID, string catID, string idvisit)
        {
            try
            {
                if (catID != null)
                {
                  
                    try {
                        int bra = Convert.ToInt32(customerID);
                        int IDV = Convert.ToInt32(idvisit);

                        var lstcat = dlipro.BI_Dim_Products
                                .Where(i => i.id_category == catID && i.id_brand == bra)
                                .Select(i => new subcategories { FirmCode = i.id_subcategory.ToString(), FirmName = i.subcategory_name, isselected = false, Category = "" })
                                .Distinct()
                                .OrderByDescending(i => i.FirmName)
                                .ToList();

                        var itemselectcat = (from br in dbcmk.FormsM_details where (br.ID_formresourcetype == 31 && br.ID_visit == IDV) select br).FirstOrDefault();
                        List<string> brandssplit = new List<string>(itemselectcat.fvalueText.Split(new string[] { "," }, StringSplitOptions.None));

                        if (itemselectcat != null)
                        {
                            foreach (var item in lstcat)
                            {
                                if (itemselectcat.fvalueText.Contains(item.FirmCode.ToString()))
                                {
                                    item.isselected = true;
                                }

                            }
                        }
                        //}
                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        string result = javaScriptSerializer.Serialize(lstcat);
                        return Json(result, JsonRequestBehavior.AllowGet);
                    } catch {
                        int bra = Convert.ToInt32(customerID);
               
                        var lstcat = dlipro.BI_Dim_Products
                                .Where(i => i.id_category == catID && i.id_brand == bra)
                                .Select(i => new subcategories { FirmCode = i.id_subcategory.ToString(), FirmName = i.subcategory_name, isselected = false, Category = "" })
                                .Distinct()
                                .OrderByDescending(i => i.FirmName)
                                .ToList();




                        //}
                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        string result = javaScriptSerializer.Serialize(lstcat);
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }

                }
            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
            return Json("error", JsonRequestBehavior.AllowGet);

        }
        public ActionResult GetRefImg(string brandID, string catID, string subcatID)
        {
            if (brandID != null)
            {
       
                int subcat = Convert.ToInt32(subcatID);
                int brand = Convert.ToInt32(brandID);
                var img = (from a in dbcmk.Activities_RefImg where (a.ID_brand == brand && a.ID_category == catID && a.ID_subcategory == subcat) select a).FirstOrDefault();
                var sr = "";
                if (img != null)
                {
                    sr = Url.Content(img.src);
                }
                string result = sr;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            return Json("error", JsonRequestBehavior.AllowGet);

        }

        //public ActionResult Getbrandcompetitors(string brandID, string idvisit)
        //{
        //    if (brandID != null)
        //    {
        //        var lstbrand = dbcmk.Brand_competitors
        //.Where(i => i.ID_brand == brandID)
        //.Select(i => new brandcompetitor { Id_brandc = i.ID_competitor.ToString(), namec = i.Name, isselected = false, Brand = "" })
        //.Distinct()
        //.OrderByDescending(i => i.namec)
        //.ToList();

        //        int IDV = Convert.ToInt32(idvisit);
        //        var itemselectbrand = (from br in dbcmk.FormsM_details where (br.ID_formresourcetype == 15 && br.ID_visit == IDV) select br).FirstOrDefault();
        //        if (itemselectbrand != null)
        //        {
        //            foreach (var item in lstbrand)
        //            {
        //                if (item.Id_brandc.ToString() == itemselectbrand.fvalueText)
        //                {
        //                    item.isselected = true;
        //                }

        //            }
        //        }
        //        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        //        string result = javaScriptSerializer.Serialize(lstbrand);
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    return Json("error", JsonRequestBehavior.AllowGet);

        //}
        public ActionResult Getbrands(string customerID, string idvisit, string catID, string subID)
        {
            try
            {
                if (customerID != null)
                {
                    try
                    {

                        int sub = Convert.ToInt32(subID);//customerID is SubcategoryID
                        int IDV = Convert.ToInt32(idvisit);
                        var lstbrands = dlipro.BI_Dim_Products
                                .Where(i => i.id_Vendor == customerID)
                                .Select(i => new brands { FirmCode = i.id_brand.ToString(), FirmName = i.Brand_Name, isselected = false, Customer = "" })
                                .Distinct()
                                .OrderByDescending(i => i.FirmName)
                                .ToList();

                        var itemselectbrand = (from br in dbcmk.FormsM_details where (br.ID_formresourcetype == 13 && br.ID_visit == IDV) select br).FirstOrDefault();
                        if (itemselectbrand != null)
                        {
                            foreach (var item in lstbrands)
                            {
                                if (item.FirmCode.ToString() == itemselectbrand.fvalueText)
                                {
                                    item.isselected = true;
                                }

                            }
                        }


                        //}
                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        string result = javaScriptSerializer.Serialize(lstbrands);
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    catch
                    {

                        int sub = Convert.ToInt32(subID);//customerID is SubcategoryID

                        var lstbrands = dlipro.BI_Dim_Products
                                .Where(i => i.id_subcategory == sub && i.id_Vendor == customerID && i.id_category == catID)
                                .Select(i => new brands { FirmCode = i.id_brand.ToString(), FirmName = i.Brand_Name, isselected = false, Customer = "" })
                                .Distinct()
                                .OrderByDescending(i => i.FirmName)
                                .ToList();

                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        string result = javaScriptSerializer.Serialize(lstbrands);
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }

                }
            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }
            return Json("error", JsonRequestBehavior.AllowGet);

        }

        //public ActionResult Getbrands(string customerID, string idvisit, string catID, string subID)
        //{
        //    try
        //    {
        //        if (customerID != null)
        //        {
        //            try
        //            {

        //                int sub = Convert.ToInt32(subID);//customerID is SubcategoryID
        //                int IDV = Convert.ToInt32(idvisit);
        //                var lstbrands = dlipro.BI_Dim_Products
        //                        .Where(i => i.id_subcategory == sub && i.id_Vendor == customerID && i.id_category == catID)
        //                        .Select(i => new brands { FirmCode = i.id_brand.ToString(), FirmName = i.Brand_Name, isselected = false, Customer = "" })
        //                        .Distinct()
        //                        .OrderByDescending(i => i.FirmName)
        //                        .ToList();

        //                var itemselectbrand = (from br in dbcmk.FormsM_details where (br.ID_formresourcetype == 13 && br.ID_visit == IDV) select br).FirstOrDefault();
        //                if (itemselectbrand != null)
        //                {
        //                    foreach (var item in lstbrands)
        //                    {
        //                        if (item.FirmCode.ToString() == itemselectbrand.fvalueText)
        //                        {
        //                            item.isselected = true;
        //                        }

        //                    }
        //                }


        //                //}
        //                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        //                string result = javaScriptSerializer.Serialize(lstbrands);
        //                return Json(result, JsonRequestBehavior.AllowGet);
        //            }
        //            catch {

        //                int sub = Convert.ToInt32(subID);//customerID is SubcategoryID

        //                var lstbrands = dlipro.BI_Dim_Products
        //                        .Where(i => i.id_subcategory == sub && i.id_Vendor == customerID && i.id_category == catID)
        //                        .Select(i => new brands { FirmCode = i.id_brand.ToString(), FirmName = i.Brand_Name, isselected = false, Customer = "" })
        //                        .Distinct()
        //                        .OrderByDescending(i => i.FirmName)
        //                        .ToList();

        //                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        //                string result = javaScriptSerializer.Serialize(lstbrands);
        //                return Json(result, JsonRequestBehavior.AllowGet);
        //            }

        //        }
        //    }
        //    catch
        //    {
        //        return Json("error", JsonRequestBehavior.AllowGet);
        //    }
        //    return Json("error", JsonRequestBehavior.AllowGet);

        //}
        public ActionResult GetDynamicProducts(string activityID, string ID_customer, string ID_category, string ID_subcategory, string ID_brand)
        {
            try
            {
                int idact = Convert.ToInt32(activityID);
                List<BI_Dim_Products> lstproduct = new List<BI_Dim_Products>();
                string vendoriD = ID_customer;
                int brand = Convert.ToInt32(ID_brand);

                List<string> subcatsplit = new List<string>(ID_subcategory.Split(new string[] { "," }, StringSplitOptions.None));

                List<int?> subInt = new List<int?>();
                foreach (var item in subcatsplit) {
                    subInt.Add(Convert.ToInt32(item));
                }
                //int sub = Convert.ToInt32(ID_subcategory);
           
                using (DLI_PROEntities dbmk = new DLI_PROEntities())
                {
                    lstproduct = (dlipro.BI_Dim_Products.Where(x => x.id_brand==brand && subInt.Contains(x.id_subcategory) && x.id_category == ID_category && x.Pepperi=="Yes" && x.Active=="Y")).OrderBy(c=>c.id_subcategory).ToList<BI_Dim_Products>();
                }

                if (lstproduct.Count > 0)
                {


                    ActivitiesM act = (from actd in dbcmk.ActivitiesM where (actd.ID_activity == idact) select actd).FirstOrDefault();
                    var countItems = (from a in dbcmk.FormsM_details where (a.ID_visit == idact) select a).Count();

                    var nuevacuenta = countItems + 20;
                    var activeSub = "";
                    var countp = 0;
                    var totalpro = lstproduct.Count();

                    var subcatid = 0;
                    var subcatname = "";

                    List<FormsM_details> detailstoinsert = new List<FormsM_details>();
                    foreach (var item in lstproduct)
                    {
                        try
                        {
                            if (countp == 0) {
                                FormsM_details detalle_subcatnuevo = new FormsM_details(); //Category


                                detalle_subcatnuevo.ID_formresourcetype = 95;
                                detalle_subcatnuevo.fsource = "";
                                detalle_subcatnuevo.fdescription = "";
                                detalle_subcatnuevo.fvalue = 0;
                                detalle_subcatnuevo.fvalueDecimal = 0;
                                detalle_subcatnuevo.fvalueText = item.subcategory_name;
                                detalle_subcatnuevo.ID_formM = act.ID_form;

                                detalle_subcatnuevo.ID_visit = idact;
                                detalle_subcatnuevo.original = false;
                                //Colocamos numero de orden
                                detalle_subcatnuevo.obj_order = nuevacuenta;
                                //Colocamos grupo si tiene
                                detalle_subcatnuevo.obj_group = Convert.ToInt32(item.id_subcategory);
                                //Colocamos ID generado por editor
                                detalle_subcatnuevo.idkey = nuevacuenta;
                                detalle_subcatnuevo.query1 = "";
                                detalle_subcatnuevo.query2 = "";
                                detalle_subcatnuevo.parent = 13;
                                detalle_subcatnuevo.ID_empresa = 11;



                                detailstoinsert.Add(detalle_subcatnuevo);

                                nuevacuenta++;


                                subcatid= Convert.ToInt32(item.id_subcategory);
                                subcatname = item.subcategory_name;
                                activeSub = item.subcategory_name;
                                
                            }
                            if (activeSub == item.subcategory_name)
                            {
                           
                            }
                            else {
                                //SUBCATEGORY
                                //inicial
                                FormsM_details detalle_subcatnuevo2 = new FormsM_details(); //Subcategory


                                detalle_subcatnuevo2.ID_formresourcetype = 95;
                                detalle_subcatnuevo2.fsource = "";
                                detalle_subcatnuevo2.fdescription = "";
                                detalle_subcatnuevo2.fvalue = 0;
                                detalle_subcatnuevo2.fvalueDecimal = 0;
                                detalle_subcatnuevo2.fvalueText = item.subcategory_name;
                                detalle_subcatnuevo2.ID_formM = act.ID_form;

                                detalle_subcatnuevo2.ID_visit = idact;
                                detalle_subcatnuevo2.original = false;
                                //Colocamos numero de orden
                                detalle_subcatnuevo2.obj_order = nuevacuenta;
                                //Colocamos grupo si tiene
                                detalle_subcatnuevo2.obj_group = Convert.ToInt32(item.id_subcategory);
                                //Colocamos ID generado por editor
                                detalle_subcatnuevo2.idkey = nuevacuenta;
                                detalle_subcatnuevo2.query1 = "";
                                detalle_subcatnuevo2.query2 = "";
                                detalle_subcatnuevo2.parent = 13;
                                detalle_subcatnuevo2.ID_empresa = 11;



                                detailstoinsert.Add(detalle_subcatnuevo2);

                                nuevacuenta++;


                                subcatid = Convert.ToInt32(item.id_subcategory);
                                subcatname = item.subcategory_name;
                                activeSub = item.subcategory_name;


                               
                            }
                            countp++;

                            FormsM_details detalle_nuevo = new FormsM_details(); //Producto


                            detalle_nuevo.ID_formresourcetype = 3;
                            detalle_nuevo.fsource = item.id;
                            detalle_nuevo.fdescription = item.Product;
                            detalle_nuevo.fvalue = 0;
                            detalle_nuevo.fvalueDecimal = 0;
                            detalle_nuevo.fvalueText = item.subcategory_name;
                            detalle_nuevo.ID_formM = act.ID_form;

                            detalle_nuevo.ID_visit = idact;
                            detalle_nuevo.original = false;
                            //Colocamos numero de orden
                            detalle_nuevo.obj_order = nuevacuenta;
                            //Colocamos grupo si tiene
                            detalle_nuevo.obj_group =Convert.ToInt32(item.id_subcategory);
                            //Colocamos ID generado por editor
                            detalle_nuevo.idkey = nuevacuenta;
                            detalle_nuevo.query1 = "";
                            detalle_nuevo.query2 = "";
                            detalle_nuevo.parent = 13;
                            detalle_nuevo.ID_empresa = 11;



                            detailstoinsert.Add(detalle_nuevo);
                            //dbcmk.SaveChanges();


                            var padrec = nuevacuenta;
                            nuevacuenta++;
                            var padredetalle = 0;
                            //Creamos los elementos que contendra cada producto (nuevo metodo 08/04/2020)
                            for (var i = 0; i < 31; i++) {
                                FormsM_details detalle_nuevodeProducto = new FormsM_details();
                                //0=¿Producto Disponible? -8
                                //1=Disponible -19
                                //2=No Disponible-19
                                //3=¿Cuantas caras tiene el producto/sku? - 17
                                //4=¿Como se encuentra actualmente la selección de precio? -8
                                //5=Precio sugerido -21
                                //6=Precio tienda -21
                                //7=¿Producto posee alguna promocion? -8
                                //8=SI -19
                                //9=NO -19
                                //10=ANALISIS DE VISIBILIDAD Y COMUNICACIÓN - 8
                                //11=¿La tienda cuenta con material pop? -8
                                //12=SI -19
                                //13=NO -19
                                //14=¿Qué tipo de material pop y exhibicion permite la tienda colocar para el producto? (se puede seleccionar más de una opción)
                                //15=Cross -16
                                //16=Bandeja -16
                                //17=Carrilera -16
                                //18=Glorificador -16
                                //19=Sticker Precio (preciadores) -16
                                //20=Cintillo (channel strips) -16
                                //21=Gráficas de piso -16
                                //22=Sticker en Nevera -16
                                //23=Displays / Racks -16
                                //24=Afiche o imágenes en paredes -16
                                //25=No permite -16
                                //26=¿En que posición de la gondola se encuetra el producto? -8
                                //27=Stretch Level -16
                                //28=Eye Level -16
                                //29=Touch Level -16
                                //30=Stoop Level -16
                                detalle_nuevodeProducto.ID_formresourcetype = 8;
                                detalle_nuevodeProducto.fsource = "";
                                detalle_nuevodeProducto.fdescription = "";
                                detalle_nuevodeProducto.fvalue = 0;
                                detalle_nuevodeProducto.fvalueDecimal = 0;
                                detalle_nuevodeProducto.fvalueText = "";
                                detalle_nuevodeProducto.parent = padrec;
                               
                                switch (i) {
                                    case 0:
                                        detalle_nuevodeProducto.ID_formresourcetype = 8;
                                        detalle_nuevodeProducto.fsource = "¿Producto Disponible?";
                                        padredetalle = nuevacuenta;
                                        break;
                                    case 1:
                                        detalle_nuevodeProducto.ID_formresourcetype = 19;
                                        detalle_nuevodeProducto.fsource = "SI";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 2:
                                        detalle_nuevodeProducto.ID_formresourcetype = 19;
                                        detalle_nuevodeProducto.fsource = "NO";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 3:
                                        detalle_nuevodeProducto.ID_formresourcetype = 17;
                                        detalle_nuevodeProducto.fsource = "¿Cuantas caras tiene el producto/sku?";
                                        break;
                                    case 4:
                                        detalle_nuevodeProducto.ID_formresourcetype = 8;
                                        detalle_nuevodeProducto.fsource = "¿Como se encuentra actualmente la selección de precio?";
                                        padredetalle = nuevacuenta;
                                        break;
                                    case 5:
                                        detalle_nuevodeProducto.ID_formresourcetype = 21;
                                        detalle_nuevodeProducto.fsource = "Precio sugerido";
                                        detalle_nuevodeProducto.fvalueDecimal = Convert.ToDecimal(item.SRP); //PRECIO SUGERIDO DESDE SAP
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 6:
                                        detalle_nuevodeProducto.ID_formresourcetype = 21;
                                        detalle_nuevodeProducto.fsource = "Precio tienda (editar solo si es diferente a precio sugerido)";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 7:
                                        detalle_nuevodeProducto.ID_formresourcetype = 8;
                                        detalle_nuevodeProducto.fsource = "¿Producto posee alguna promocion?";
                                        padredetalle = nuevacuenta;
                                        break;
                                    case 8:
                                        detalle_nuevodeProducto.ID_formresourcetype = 19;
                                        detalle_nuevodeProducto.fsource = "SI";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 9:
                                        detalle_nuevodeProducto.ID_formresourcetype = 19;
                                        detalle_nuevodeProducto.fsource = "NO";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 10:
                                        detalle_nuevodeProducto.ID_formresourcetype = 8;
                                        detalle_nuevodeProducto.fsource = "ANALISIS DE VISIBILIDAD Y COMUNICACIÓN";
                                        break;
                                    case 11:
                                        detalle_nuevodeProducto.ID_formresourcetype = 8;
                                        detalle_nuevodeProducto.fsource = "¿La tienda cuenta con material pop?";
                                        padredetalle = nuevacuenta;
                                        break;
                                    case 12:
                                        detalle_nuevodeProducto.ID_formresourcetype = 19;
                                        detalle_nuevodeProducto.fsource = "SI";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 13:
                                        detalle_nuevodeProducto.ID_formresourcetype = 19;
                                        detalle_nuevodeProducto.fsource = "NO";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 14:
                                        detalle_nuevodeProducto.ID_formresourcetype = 8;
                                        detalle_nuevodeProducto.fsource = "¿Qué tipo de material pop y exhibicion permite la tienda colocar para el producto? (se puede seleccionar más de una opción)";
                                        padredetalle = nuevacuenta;
                                        break;
                                    case 15:
                                        detalle_nuevodeProducto.ID_formresourcetype = 16;
                                        detalle_nuevodeProducto.fsource = "Cross";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 16:
                                        detalle_nuevodeProducto.ID_formresourcetype = 16;
                                        detalle_nuevodeProducto.fsource = "Bandeja";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 17:
                                        detalle_nuevodeProducto.ID_formresourcetype = 16;
                                        detalle_nuevodeProducto.fsource = "Carrilera";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 18:
                                        detalle_nuevodeProducto.ID_formresourcetype = 16;
                                        detalle_nuevodeProducto.fsource = "Glorificador";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 19:
                                        detalle_nuevodeProducto.ID_formresourcetype = 16;
                                        detalle_nuevodeProducto.fsource = "Sticker Precio (preciadores)";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 20:
                                        detalle_nuevodeProducto.ID_formresourcetype = 16;
                                        detalle_nuevodeProducto.fsource = "Cintillo (channel strips)";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 21:
                                        detalle_nuevodeProducto.ID_formresourcetype = 16;
                                        detalle_nuevodeProducto.fsource = "Gráficas de piso";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 22:
                                        detalle_nuevodeProducto.ID_formresourcetype = 16;
                                        detalle_nuevodeProducto.fsource = "Sticker en Nevera";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 23:
                                        detalle_nuevodeProducto.ID_formresourcetype = 16;
                                        detalle_nuevodeProducto.fsource = "Displays / Racks";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 24:
                                        detalle_nuevodeProducto.ID_formresourcetype = 16;
                                        detalle_nuevodeProducto.fsource = "Afiche o imágenes en paredes";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 25:
                                        detalle_nuevodeProducto.ID_formresourcetype = 16;
                                        detalle_nuevodeProducto.fsource = "No permite";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 26:
                                        detalle_nuevodeProducto.ID_formresourcetype = 8;
                                        detalle_nuevodeProducto.fsource = "¿En que posición de la gondola se encuetra el producto?";
                                        padredetalle = nuevacuenta;
                                        break;
                                    case 27:
                                        detalle_nuevodeProducto.ID_formresourcetype = 19;
                                        detalle_nuevodeProducto.fsource = "Stretch Level";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 28:
                                        detalle_nuevodeProducto.ID_formresourcetype = 19;
                                        detalle_nuevodeProducto.fsource = "Eye Level";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 29:
                                        detalle_nuevodeProducto.ID_formresourcetype = 19;
                                        detalle_nuevodeProducto.fsource = "Touch Level";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                    case 30:
                                        detalle_nuevodeProducto.ID_formresourcetype = 19;
                                        detalle_nuevodeProducto.fsource = "Stoop Level";
                                        detalle_nuevodeProducto.parent = padredetalle;
                                        break;
                                }
                          

                                //Deatalles que no se evaluan
                                detalle_nuevodeProducto.ID_formM = act.ID_form;
                                detalle_nuevodeProducto.ID_visit = idact;
                                detalle_nuevodeProducto.original = false;
                                //Colocamos numero de orden
                                detalle_nuevodeProducto.obj_order = nuevacuenta;
                                //Colocamos grupo si tiene
                                detalle_nuevodeProducto.obj_group = 0;
                                //Colocamos ID generado por editor
                                detalle_nuevodeProducto.idkey = nuevacuenta;
                                detalle_nuevodeProducto.query1 = "";
                                detalle_nuevodeProducto.query2 = "";
                                detalle_nuevodeProducto.ID_empresa = 11;



                                detailstoinsert.Add(detalle_nuevodeProducto);
                                nuevacuenta++;
                            }

                        }
                        catch (Exception ex)
                        {
                            var error = ex.Message;

                        }

                    }
                    dbcmk.BulkInsert(detailstoinsert);
                    //FormsM_details lastitem = (from a in dbcmk.FormsM_details where (a.ID_visit == idact && a.idkey == (countItems - 2)) select a).FirstOrDefault();

                    //lastitem.obj_order = nuevacuenta + 200;
                    //lastitem.idkey = nuevacuenta + 200;
                    //dbcmk.Entry(lastitem).State = EntityState.Modified;
                    ////dbcmk.SaveChanges();
                    //nuevacuenta++;
                    //FormsM_details lastitem2 = (from a in dbcmk.FormsM_details where (a.ID_visit == idact && a.idkey == (countItems - 1)) select a).FirstOrDefault();

                    //lastitem2.obj_order = nuevacuenta + 200;
                    //lastitem2.idkey = nuevacuenta + 200;
                    //dbcmk.Entry(lastitem2).State = EntityState.Modified;
                    ////dbcmk.SaveChanges();
                    //nuevacuenta++;
                    //FormsM_details lastitem3 = (from a in dbcmk.FormsM_details where (a.ID_visit == idact && a.idkey == countItems) select a).FirstOrDefault();

                    //lastitem3.obj_order = nuevacuenta + 200;
                    //lastitem3.idkey = nuevacuenta + 200;
                    //dbcmk.Entry(lastitem3).State = EntityState.Modified;
                    //dbcmk.SaveChanges();

                    string result = "Success";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else {

                    string result = "Nodata";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                string result = "Error";
                return Json(result, JsonRequestBehavior.AllowGet);
            }



        }

        public ActionResult GetDynamicBarcodeforKlassForm(string activityID)
        {
            try
            {
                int idact = Convert.ToInt32(activityID);
          
                    ActivitiesM act = (from actd in dbcmk.ActivitiesM where (actd.ID_activity == idact) select actd).FirstOrDefault();
                    var countItems = (from a in dbcmk.FormsM_details where (a.ID_visit == idact) select a).Count();

                    var nuevacuenta = countItems + 4;

                        try
                        {
                            FormsM_details detalle_nuevo = new FormsM_details(); //Producto


                            detalle_nuevo.ID_formresourcetype = 28;
                            detalle_nuevo.fsource = "";
                            detalle_nuevo.fdescription = "";
                            detalle_nuevo.fvalue = 0;
                            detalle_nuevo.fvalueDecimal = 0;
                            detalle_nuevo.fvalueText = "";
                            detalle_nuevo.ID_formM = act.ID_form;

                            detalle_nuevo.ID_visit = idact;
                            detalle_nuevo.original = false;
                            //Colocamos numero de orden
                            detalle_nuevo.obj_order = nuevacuenta;
                            //Colocamos grupo si tiene
                            detalle_nuevo.obj_group = 0;
                            //Colocamos ID generado por editor
                            detalle_nuevo.idkey = nuevacuenta;
                            detalle_nuevo.query1 = "";
                            detalle_nuevo.query2 = "";
                            detalle_nuevo.parent = 0;
                            detalle_nuevo.ID_empresa = 11;



                            dbcmk.FormsM_details.Add(detalle_nuevo);
                            dbcmk.SaveChanges();
                    nuevacuenta++;

                          

                        }
                        catch (Exception ex)
                        {
                            var error = ex.Message;

                        }

                    

                    FormsM_details lastitem = (from a in dbcmk.FormsM_details where (a.ID_visit == idact && a.ID_formresourcetype== 8 && a.fsource.Contains("AFTER")) select a).FirstOrDefault();

                    lastitem.obj_order = nuevacuenta + 400;
                    lastitem.idkey = nuevacuenta + 400;
                    dbcmk.Entry(lastitem).State = EntityState.Modified;
                    dbcmk.SaveChanges();
                    nuevacuenta++;
                    FormsM_details lastitem2 = (from a in dbcmk.FormsM_details where (a.ID_visit == idact && a.ID_formresourcetype == 5 && a.fdescription.Contains("PICTURE 2(AFTER)")) select a).FirstOrDefault();

                    lastitem2.obj_order = nuevacuenta + 400;
                    lastitem2.idkey = nuevacuenta + 400;
                    dbcmk.Entry(lastitem2).State = EntityState.Modified;
                    dbcmk.SaveChanges();


                    string result = "Success";
                    return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                string result = "Error";
                return Json(result, JsonRequestBehavior.AllowGet);
            }



        }

        //public ActionResult Getdisplays(string vendorID)
        //{
        //    List<OITM> lstproduct = new List<OITM>();
        //    string vendoriD = vendorID;
        //    using (COM_MKEntities dbmk = new COM_MKEntities())
        //    {
        //        lstproduct = (dbmk.OITM.Where(x => x.U_CustomerCM == vendoriD && x.ItemCode.Contains("DIS%"))).ToList<OITM>();
        //    }
        //    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        //    string result = javaScriptSerializer.Serialize(lstproduct);
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult GetDynamicProducts(string activityID, string ID_customer)
        //{
        //    try
        //    {
        //        int idact = Convert.ToInt32(activityID);
        //        List<OITM> lstproduct = new List<OITM>();
        //        string vendoriD = ID_customer;
        //        using (COM_MKEntities dbmk = new COM_MKEntities())
        //        {
        //            lstproduct = (dbmk.OITM.Where(x => x.U_CustomerCM == vendoriD)).ToList<OITM>();
        //        }

        //        ActivitiesM act = (from actd in dbcmk.ActivitiesM where (actd.ID_activity == idact) select actd).FirstOrDefault();
        //        var countItems = (from a in dbcmk.FormsM_details where (a.ID_visit == idact) select a).Count();

        //        var nuevacuenta = countItems + 4;

        //        foreach (var item in lstproduct)
        //        {
        //            try
        //            {
        //                FormsM_details detalle_nuevo = new FormsM_details();


        //                detalle_nuevo.ID_formresourcetype = 3;
        //                detalle_nuevo.fsource = item.ItemCode;
        //                detalle_nuevo.fdescription = item.ItemName;
        //                detalle_nuevo.fvalue = 0;
        //                detalle_nuevo.fvalueDecimal = 0;
        //                detalle_nuevo.fvalueText = "";
        //                detalle_nuevo.ID_formM = act.ID_form;

        //                detalle_nuevo.ID_visit = idact;
        //                detalle_nuevo.original = false;
        //                //Colocamos numero de orden
        //                detalle_nuevo.obj_order = nuevacuenta;
        //                //Colocamos grupo si tiene
        //                detalle_nuevo.obj_group = 0;
        //                //Colocamos ID generado por editor
        //                detalle_nuevo.idkey = nuevacuenta;
        //                detalle_nuevo.query1 = "";
        //                detalle_nuevo.query2 = "";
        //                detalle_nuevo.parent = 0;
        //                detalle_nuevo.ID_empresa = GlobalVariables.ID_EMPRESA_USUARIO;



        //                dbcmk.FormsM_details.Add(detalle_nuevo);
        //                dbcmk.SaveChanges();
        //                nuevacuenta++;
        //            }
        //            catch (Exception ex)
        //            {
        //                var error = ex.Message;

        //            }

        //        }

        //        FormsM_details lastitem = (from a in dbcmk.FormsM_details where (a.ID_visit == idact && a.idkey == (countItems - 2)) select a).FirstOrDefault();

        //        lastitem.obj_order = nuevacuenta + 1;
        //        lastitem.idkey = nuevacuenta + 1;
        //        dbcmk.Entry(lastitem).State = EntityState.Modified;
        //        dbcmk.SaveChanges();
        //        nuevacuenta++;
        //        FormsM_details lastitem2 = (from a in dbcmk.FormsM_details where (a.ID_visit == idact && a.idkey == (countItems - 1)) select a).FirstOrDefault();

        //        lastitem2.obj_order = nuevacuenta + 1;
        //        lastitem2.idkey = nuevacuenta + 1;
        //        dbcmk.Entry(lastitem2).State = EntityState.Modified;
        //        dbcmk.SaveChanges();
        //        nuevacuenta++;
        //        FormsM_details lastitem3 = (from a in dbcmk.FormsM_details where (a.ID_visit == idact && a.idkey == countItems) select a).FirstOrDefault();

        //        lastitem3.obj_order = nuevacuenta + 1;
        //        lastitem3.idkey = nuevacuenta + 1;
        //        dbcmk.Entry(lastitem3).State = EntityState.Modified;
        //        dbcmk.SaveChanges();

        //        var customer = (from cust in COM_MKdb.OCRD where (cust.CardCode == ID_customer) select cust).FirstOrDefault();
        //        act.ID_customer = ID_customer;
        //        act.Customer = customer.CardName;
        //        db.Entry(act).State = EntityState.Modified;
        //        dbcmk.SaveChanges();


        //        string result = "Success";
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    catch
        //    {
        //        string result = "Error";
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }



        //}

        //public ActionResult GetDynamicProductsByBrand(string activityID, string ID_customer, string ID_brand)
        //{
        //    try
        //    {
        //        int idact = Convert.ToInt32(activityID);
        //        List<OITM> lstproduct = new List<OITM>();
        //        string vendoriD = ID_customer;
        //        int brand = Convert.ToInt32(ID_brand);
        //        using (COM_MKEntities dbmk = new COM_MKEntities())
        //        {
        //            lstproduct = (dbmk.OITM.Where(x => x.U_CustomerCM == vendoriD && x.FirmCode == brand)).ToList<OITM>();
        //        }

        //        ActivitiesM act = (from actd in dbcmk.ActivitiesM where (actd.ID_activity == idact) select actd).FirstOrDefault();
        //        var countItems = (from a in dbcmk.FormsM_details where (a.ID_visit == idact) select a).Count();

        //        var nuevacuenta = countItems + 2;

        //        foreach (var item in lstproduct)
        //        {
        //            try
        //            {
        //                FormsM_details detalle_nuevo = new FormsM_details();


        //                detalle_nuevo.ID_formresourcetype = 3;
        //                detalle_nuevo.fsource = item.ItemCode;
        //                detalle_nuevo.fdescription = item.ItemName;
        //                detalle_nuevo.fvalue = 0;
        //                detalle_nuevo.fvalueDecimal = 0;
        //                detalle_nuevo.fvalueText = "";
        //                detalle_nuevo.ID_formM = act.ID_form;

        //                detalle_nuevo.ID_visit = idact;
        //                detalle_nuevo.original = false;
        //                //Colocamos numero de orden
        //                detalle_nuevo.obj_order = nuevacuenta;
        //                //Colocamos grupo si tiene
        //                detalle_nuevo.obj_group = 0;
        //                //Colocamos ID generado por editor
        //                detalle_nuevo.idkey = nuevacuenta;
        //                detalle_nuevo.query1 = "";
        //                detalle_nuevo.query2 = "";
        //                detalle_nuevo.parent = 0;
        //                detalle_nuevo.ID_empresa = GlobalVariables.ID_EMPRESA_USUARIO;



        //                dbcmk.FormsM_details.Add(detalle_nuevo);
        //                dbcmk.SaveChanges();
        //                nuevacuenta++;

        //                FormsM_details detalle_nuevo2 = new FormsM_details();


        //                detalle_nuevo2.ID_formresourcetype = 22;
        //                detalle_nuevo2.fsource = "Expiration Date";
        //                detalle_nuevo2.fdescription = "";
        //                detalle_nuevo2.fvalue = 0;
        //                detalle_nuevo2.fvalueDecimal = 0;
        //                detalle_nuevo2.fvalueText = "";
        //                detalle_nuevo2.ID_formM = act.ID_form;

        //                detalle_nuevo2.ID_visit = idact;
        //                detalle_nuevo2.original = false;
        //                //Colocamos numero de orden
        //                detalle_nuevo2.obj_order = nuevacuenta;
        //                //Colocamos grupo si tiene
        //                detalle_nuevo2.obj_group = 0;
        //                //Colocamos ID generado por editor
        //                detalle_nuevo2.idkey = nuevacuenta;
        //                detalle_nuevo2.query1 = "";
        //                detalle_nuevo2.query2 = "";
        //                detalle_nuevo2.parent = 0;
        //                detalle_nuevo2.ID_empresa = GlobalVariables.ID_EMPRESA_USUARIO;



        //                dbcmk.FormsM_details.Add(detalle_nuevo2);
        //                dbcmk.SaveChanges();
        //                nuevacuenta++;

        //            }
        //            catch (Exception ex)
        //            {
        //                var error = ex.Message;

        //            }

        //        }

        //        //FormsM_details lastitem = (from a in dbcmk.FormsM_details where (a.ID_visit == idact && a.idkey == (countItems - 2)) select a).FirstOrDefault();

        //        //lastitem.obj_order = nuevacuenta + 1;
        //        //lastitem.idkey = nuevacuenta + 1;
        //        //dbcmk.Entry(lastitem).State = EntityState.Modified;
        //        //dbcmk.SaveChanges();
        //        //nuevacuenta++;
        //        FormsM_details lastitem2 = (from a in dbcmk.FormsM_details where (a.ID_visit == idact && a.idkey == (countItems - 1)) select a).FirstOrDefault();

        //        lastitem2.obj_order = nuevacuenta + 1;
        //        lastitem2.idkey = nuevacuenta + 1;
        //        dbcmk.Entry(lastitem2).State = EntityState.Modified;
        //        dbcmk.SaveChanges();
        //        nuevacuenta++;
        //        FormsM_details lastitem3 = (from a in dbcmk.FormsM_details where (a.ID_visit == idact && a.idkey == countItems) select a).FirstOrDefault();

        //        lastitem3.obj_order = nuevacuenta + 1;
        //        lastitem3.idkey = nuevacuenta + 1;
        //        dbcmk.Entry(lastitem3).State = EntityState.Modified;
        //        dbcmk.SaveChanges();

        //        var customer = (from cust in COM_MKdb.OCRD where (cust.CardCode == ID_customer) select cust).FirstOrDefault();
        //        act.ID_customer = ID_customer;
        //        act.Customer = customer.CardName;
        //        dbcmk.Entry(act).State = EntityState.Modified;
        //        dbcmk.SaveChanges();


        //        string result = "Success";
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    catch
        //    {
        //        string result = "Error";
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }



        //}

        //public ActionResult Getgifts(string vendorID)
        //{
        //    List<OITM> lstproduct = new List<OITM>();
        //    string vendoriD = vendorID;
        //    using (COM_MKEntities dbmk = new COM_MKEntities())
        //    {
        //        lstproduct = (dbmk.OITM.Where(x => x.U_CustomerCM == vendoriD && x.ItmsGrpCod == 108)).ToList<OITM>();
        //    }
        //    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        //    string result = javaScriptSerializer.Serialize(lstproduct);
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult Getproductline(string brandID, string idvisit)
        //{
        //    //List<view_CMKEditorB> lstproductline = new List<view_CMKEditorB>();

        //    //using (COM_MKEntities dbmk = new COM_MKEntities())
        //    //{
        //    //    lstproductline = (dbmk.view_CMKEditorB.Where(x => x.FirmCode.ToString() == brandID)).ToList<view_CMKEditorB>();
        //    //}
        //    if (brandID != null)
        //    {
        //        var lstproductline = COM_MKdb.view_CMKEditorB
        //.Where(i => i.Id_subcategory != null && i.FirmCode.ToString() == brandID)
        //.Select(i => new productline { Id_subcategory = i.Id_subcategory, SubCategory = i.SubCategory, isselected = false, Brand = "" })
        //.Distinct()
        //.OrderByDescending(i => i.SubCategory)
        //.ToList();

        //        int IDV = Convert.ToInt32(idvisit);
        //        var itemselectbrand = (from br in dbcmk.FormsM_details where (br.ID_formresourcetype == 14 && br.ID_visit == IDV) select br).FirstOrDefault();
        //        if (itemselectbrand != null)
        //        {
        //            foreach (var item in lstproductline)
        //            {
        //                if (item.Id_subcategory.ToString() == itemselectbrand.fvalueText)
        //                {
        //                    item.isselected = true;
        //                }

        //            }
        //        }
        //        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        //        string result = javaScriptSerializer.Serialize(lstproductline);
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    return Json("error", JsonRequestBehavior.AllowGet);

        //}

        //public ActionResult Getproducts(string vendorID)
        //{
        //    List<OITM> lstproduct = new List<OITM>();
        //    string vendoriD = vendorID;
        //    using (COM_MKEntities dbmk = new COM_MKEntities())
        //    {
        //        lstproduct = (dbmk.OITM.Where(x => x.U_CustomerCM == vendoriD)).OrderBy(x => x.ItemName).ToList<OITM>();
        //    }

        //    foreach (var item in lstproduct)
        //    {

        //        item.ItemName = item.ItemName.Replace("\'", "");
        //    }




        //    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        //    string result = javaScriptSerializer.Serialize(lstproduct);
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult Getsamples(string vendorID)
        //{
        //    List<OITM> lstproduct = new List<OITM>();
        //    string vendoriD = vendorID;
        //    using (COM_MKEntities dbmk = new COM_MKEntities())
        //    {
        //        lstproduct = (dbmk.OITM.Where(x => x.U_CustomerCM == vendoriD && x.ItmsGrpCod == 107)).ToList<OITM>();
        //    }
        //    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        //    string result = javaScriptSerializer.Serialize(lstproduct);
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult Save_activity(string id, List<MyObj_formtemplate> objects, string lat, string lng, string check_in)
        {
            List<FormsM_details> detailsForm = Session["detailsForm"] as List<FormsM_details>;
            try
            {
                int IDU = Convert.ToInt32(Session["IDusuario"]);
                if (id != null)
                {
                    int act = Convert.ToInt32(id);
                    //ActivitiesM activity = db.ActivitiesM.Find(act);
                    //activity.check_out = Convert.ToDateTime(check_in);
                    //db.Entry(activity).State = EntityState.Modified;             
                    //db.SaveChanges();

                    //if (lat != null || lat != "")
                    //{
                    //    //Guardamos el log de la actividad
                    //    ActivitiesM_log nuevoLog = new ActivitiesM_log();
                    //    nuevoLog.latitude = lat;
                    //    nuevoLog.longitude = lng;
                    //    nuevoLog.ID_usuario = IDU;
                    //    nuevoLog.ID_activity = Convert.ToInt32(id);
                    //    nuevoLog.fecha_conexion = Convert.ToDateTime(check_in);
                    //    nuevoLog.query1 = "";
                    //    nuevoLog.query2 = "";
                    //    nuevoLog.action = "SAVE ACTIVITY - " + activity.description;
                    //    nuevoLog.ip = "";
                    //    nuevoLog.hostname = "";
                    //    nuevoLog.typeh = "";
                    //    nuevoLog.continent_name = "";
                    //    nuevoLog.country_code = "";
                    //    nuevoLog.country_name = "";
                    //    nuevoLog.region_code = "";
                    //    nuevoLog.region_name = "";
                    //    nuevoLog.city = "";

                    //    db.ActivitiesM_log.Add(nuevoLog);
                    //    db.SaveChanges();
                    //}


                    //Guardamos el detalle del formlario
                    if (objects != null)
                    {
                        foreach (var item in objects)
                        {
                            int IDItem = Convert.ToInt32(item.id);
                            FormsM_details detail = (from f in detailsForm where (f.ID_visit == act && f.idkey == IDItem) select f).FirstOrDefault();
                            if (detail == null)
                            {

                            }
                            else
                            {
                                //if (detail.ID_formresourcetype == 3 || detail.ID_formresourcetype == 4 || detail.ID_formresourcetype == 10)//Products, Samples,Gift
                                //{
                                //    if (item.value == "" || item.value == null) { item.value = "0"; }
                                //    detail.fvalue = Convert.ToInt32(item.value);

                                //    db.Entry(detail).State = EntityState.Modified;
                                //    db.SaveChanges();

                                //}
                                //else 
                                if (detail.ID_formresourcetype == 5) //Picture
                                {
                                    //
                                    if (item.value == "100")
                                    {
                                        var path = detail.fsource;
                                        //eliminamos la ruta
                                        detail.fsource = "";

                                        dbcmk.Entry(detail).State = EntityState.Modified;



                                        if (System.IO.File.Exists(Server.MapPath(path)))
                                        {
                                            try
                                            {
                                                System.IO.File.Delete(Server.MapPath(path));
                                            }
                                            catch (System.IO.IOException e)
                                            {
                                                Console.WriteLine(e.Message);

                                            }
                                        }
                                    }




                                }
                                else if (detail.ID_formresourcetype == 9) //Input text y Electronic Signature
                                {

                                    if (item.value == "" || item.value == null) { item.value = ""; }
                                    if (detail.fsource != item.value)
                                    {
                                        detail.fsource = item.value;

                                        dbcmk.Entry(detail).State = EntityState.Modified;

                                    }

                                }

                                //}
                                else
                                {
                                    //No hacemos nada porque no esta registrado
                                }

                            }


                        }
                        dbcmk.SaveChanges();

                        Session["detailsForm"] = detailsForm;
                    }

                    return Json(new { Result = "Success" });
                }
                return Json(new { Result = "Warning" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "Warning" + ex.Message });
            }

        }

        public JsonResult Save_activitySurvey(string id, List<MyObj_formtemplate> objects, string lat, string lng, string check_in)
        {
            List<FormsM_detailsTasks> detailsForm = Session["detailsForm"] as List<FormsM_detailsTasks>;
            try
            {
                int IDU = Convert.ToInt32(Session["IDusuario"]);
                if (id != null)
                {
                    int act = Convert.ToInt32(id);
                    //ActivitiesM activity = db.ActivitiesM.Find(act);
                    //activity.check_out = Convert.ToDateTime(check_in);
                    //db.Entry(activity).State = EntityState.Modified;             
                    //db.SaveChanges();

                    //if (lat != null || lat != "")
                    //{
                    //    //Guardamos el log de la actividad
                    //    ActivitiesM_log nuevoLog = new ActivitiesM_log();
                    //    nuevoLog.latitude = lat;
                    //    nuevoLog.longitude = lng;
                    //    nuevoLog.ID_usuario = IDU;
                    //    nuevoLog.ID_activity = Convert.ToInt32(id);
                    //    nuevoLog.fecha_conexion = Convert.ToDateTime(check_in);
                    //    nuevoLog.query1 = "";
                    //    nuevoLog.query2 = "";
                    //    nuevoLog.action = "SAVE ACTIVITY - " + activity.description;
                    //    nuevoLog.ip = "";
                    //    nuevoLog.hostname = "";
                    //    nuevoLog.typeh = "";
                    //    nuevoLog.continent_name = "";
                    //    nuevoLog.country_code = "";
                    //    nuevoLog.country_name = "";
                    //    nuevoLog.region_code = "";
                    //    nuevoLog.region_name = "";
                    //    nuevoLog.city = "";

                    //    db.ActivitiesM_log.Add(nuevoLog);
                    //    db.SaveChanges();
                    //}


                    //Guardamos el detalle del formlario
                    if (objects != null)
                    {
                        foreach (var item in objects)
                        {
                            int IDItem = Convert.ToInt32(item.id);
                            FormsM_detailsTasks detail = (from f in detailsForm where (f.ID_visit == act && f.idkey == IDItem) select f).FirstOrDefault();
                            if (detail == null)
                            {

                            }
                            else
                            {
                                //if (detail.ID_formresourcetype == 3 || detail.ID_formresourcetype == 4 || detail.ID_formresourcetype == 10)//Products, Samples,Gift
                                //{
                                //    if (item.value == "" || item.value == null) { item.value = "0"; }
                                //    detail.fvalue = Convert.ToInt32(item.value);

                                //    db.Entry(detail).State = EntityState.Modified;
                                //    db.SaveChanges();

                                //}
                                //else 
                                if (detail.ID_formresourcetype == 5) //Picture
                                {
                                    //
                                    if (item.value == "100")
                                    {
                                        var path = detail.fsource;
                                        //eliminamos la ruta
                                        detail.fsource = "";

                                        dbcmk.Entry(detail).State = EntityState.Modified;



                                        if (System.IO.File.Exists(Server.MapPath(path)))
                                        {
                                            try
                                            {
                                                System.IO.File.Delete(Server.MapPath(path));
                                            }
                                            catch (System.IO.IOException e)
                                            {
                                                Console.WriteLine(e.Message);

                                            }
                                        }
                                    }




                                }
                                else if (detail.ID_formresourcetype == 9) //Input text y Electronic Signature
                                {

                                    if (item.value == "" || item.value == null) { item.value = ""; }
                                    if (detail.fsource != item.value)
                                    {
                                        detail.fsource = item.value;

                                        dbcmk.Entry(detail).State = EntityState.Modified;

                                    }

                                }
                                else if (detail.ID_formresourcetype == 38) //Input text y Electronic Signature
                                {

                                    if (item.value == "" || item.value == null) { item.value = ""; }
                              
                                        detail.fvalueText = item.value;

                                        dbcmk.Entry(detail).State = EntityState.Modified;

                                    

                                }

                                //}
                                else
                                {
                                    //No hacemos nada porque no esta registrado
                                }

                            }


                        }
                        dbcmk.SaveChanges();

                        Session["detailsForm"] = detailsForm;
                    }

                    return Json(new { Result = "Success" });
                }
                return Json(new { Result = "Warning" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "Warning" + ex.Message });
            }

        }


        public JsonResult Save_activityByitem(string id, List<MyObj_formtemplate> objects)
        {
            try
            {
                List<FormsM_details> detailsForm = Session["detailsForm"] as List<FormsM_details>;
                int act = Convert.ToInt32(id);
                if (detailsForm != null)
                {

                }
                else
                {
                    using (var dbs = new dbComerciaEntities())
                    {

                        detailsForm = dbs.FormsM_details.Where(a => a.ID_visit == act).ToList();
                    }

                }
                //int IDU = Convert.ToInt32(Session["IDusuario"]);
                if (id != null)
                    {
                        
                        //Guardamos el detalle del formlario
                        foreach (var item in objects)
                        {
                            int IDItem = Convert.ToInt32(item.id);
                            var detail = (from f in detailsForm where (f.ID_visit == act && f.idkey == IDItem) select f).FirstOrDefault();
                            if (detail == null)
                            {

                            }
                            else
                            {
                                if (detail.ID_formresourcetype == 3 || detail.ID_formresourcetype == 4 || detail.ID_formresourcetype == 10)//Products, Samples,Gift
                                {
                                    if (item.value == "" || item.value == null) { item.value = "0"; }

                                    if (detail.fvalue != Convert.ToInt32(item.value))
                                    {
                                        detail.fvalue = Convert.ToInt32(item.value);

                                        dbcmk.Entry(detail).State = EntityState.Modified;
                                        dbcmk.SaveChanges();


                                    }



                                }
                                else if (detail.ID_formresourcetype == 5) //Picture
                                {
                                    if (item.value == "100")
                                    {
                                        var path = detail.fsource;
                                        //eliminamos la ruta
                                        detail.fsource = "";

                                        dbcmk.Entry(detail).State = EntityState.Modified;
                                        dbcmk.SaveChanges();


                                        if (System.IO.File.Exists(Server.MapPath(path)))
                                        {
                                            try
                                            {
                                                System.IO.File.Delete(Server.MapPath(path));
                                            }
                                            catch (System.IO.IOException e)
                                            {
                                                Console.WriteLine(e.Message);

                                            }
                                        }
                                    }



                                }
                                else if (detail.ID_formresourcetype == 6 || detail.ID_formresourcetype == 9) //Input text y Electronic Signature
                                {

                                    if (item.value == "" || item.value == null) { item.value = ""; }
                                    if (detail.fsource != item.value)
                                    {
                                        detail.fsource = item.value;

                                        dbcmk.Entry(detail).State = EntityState.Modified;
                                        dbcmk.SaveChanges();
                                    }

                                }
                                else if (detail.ID_formresourcetype == 28) //BARCODE
                                {

                                    if (item.value == "" || item.value == null) { item.value = ""; }
                                    if (detail.fsource != item.value)
                                    {
                                        detail.fsource = item.value;

                                        dbcmk.Entry(detail).State = EntityState.Modified;
                                        dbcmk.SaveChanges();
                                    }

                                }

                                else if (detail.ID_formresourcetype == 18) //Input number
                                {

                                    if (item.value == "" || item.value == null) { item.value = "0"; }
                                    if (detail.fvalueDecimal != Convert.ToDecimal(item.value))
                                    {
                                        detail.fvalueDecimal = Convert.ToDecimal(item.value);

                                        dbcmk.Entry(detail).State = EntityState.Modified;
                                        dbcmk.SaveChanges();
                                    }

                                }
                                else if (detail.ID_formresourcetype == 21) // currency
                                {

                                    if (item.value == "" || item.value == null) { item.value = "0"; }

                                    if (detail.fvalueDecimal != Convert.ToDecimal(item.value))
                                    {
                                        detail.fvalueDecimal = Convert.ToDecimal(item.value);

                                        dbcmk.Entry(detail).State = EntityState.Modified;
                                        dbcmk.SaveChanges();
                                    }

                                }

                                else if (detail.ID_formresourcetype == 22) // Date
                                {

                                    if (item.value == "" || item.value == null) { item.value = ""; }

                                    try
                                    {
                                        detail.fvalueText = Convert.ToDateTime(item.value).ToShortDateString();
                                    }
                                    catch
                                    {
                                        detail.fvalueText = "";
                                    }


                                    dbcmk.Entry(detail).State = EntityState.Modified;
                                    dbcmk.SaveChanges();
                                    //db.Entry(detail).State = EntityState.Modified;
                                    //db.SaveChanges();
                                }

                                //Select, Customer, Brands,Product line, Brand Competitors 
                                else if (detail.ID_formresourcetype == 17 || detail.ID_formresourcetype == 12 || detail.ID_formresourcetype == 13 || detail.ID_formresourcetype == 14 || detail.ID_formresourcetype == 15
                                    || detail.ID_formresourcetype == 30 || detail.ID_formresourcetype == 31)
                                {
                                    if (detail.fvalueText != item.value)
                                    {
                                        detail.fvalueText = item.value; //Lo guardamos como texto por si colocan ID tipo cadena
                                        detail.fdescription = item.text;

                                        dbcmk.Entry(detail).State = EntityState.Modified;
                                        dbcmk.SaveChanges();
                                    }

                                }

                                else if (detail.ID_formresourcetype == 19 || detail.ID_formresourcetype == 16) //checkbox,radio
                                {

                                    if (item.value == "" || item.value == null) { item.value = "false"; }
                                    int seleccionado = 0;
                                    if (item.value == "false")
                                    {
                                        seleccionado = 0;
                                    }
                                    else if (item.value == "true")
                                    {
                                        seleccionado = 1;
                                    }

                                    if (detail.fvalue != seleccionado)
                                    {
                                        detail.fvalue = seleccionado; //Lo guardamos como entero

                                        dbcmk.Entry(detail).State = EntityState.Modified;
                                        dbcmk.SaveChanges();
                                    }


                                }
                                else
                                {
                                    //No hacemos nada porque no esta registrado
                                }

                            }


                        }
                        Session["detailsForm"] = detailsForm;

                        return Json(new { Result = "Success" });
                    }
                //}
                return Json(new { Result = "Warning" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "Warning" + ex.Message });
            }

        }


        public JsonResult Save_activityByitemSurvey(string id, List<MyObj_formtemplate> objects)
        {
            try
            {
                List<FormsM_detailsTasks> detailsForm = Session["detailsForm"] as List<FormsM_detailsTasks>;
                int act = Convert.ToInt32(id);
                if (detailsForm != null)
                {

                }
                else
                {
                    using (var dbs = new dbComerciaEntities())
                    {

                        detailsForm = dbs.FormsM_detailsTasks.Where(a => a.ID_visit == act).ToList();
                    }

                }
                //int IDU = Convert.ToInt32(Session["IDusuario"]);
                if (id != null)
                {

                    //Guardamos el detalle del formlario
                    foreach (var item in objects)
                    {
                        int IDItem = Convert.ToInt32(item.id);
                        var detail = (from f in detailsForm where (f.ID_visit == act && f.idkey == IDItem) select f).FirstOrDefault();
                        if (detail == null)
                        {

                        }
                        else
                        {
                            if (detail.ID_formresourcetype == 3 || detail.ID_formresourcetype == 4 || detail.ID_formresourcetype == 10)//Products, Samples,Gift
                            {
                                if (item.value == "" || item.value == null) { item.value = "0"; }

                                if (detail.fvalue != Convert.ToInt32(item.value))
                                {
                                    detail.fvalue = Convert.ToInt32(item.value);

                                    dbcmk.Entry(detail).State = EntityState.Modified;
                                    dbcmk.SaveChanges();


                                }



                            }
                            else if (detail.ID_formresourcetype == 5) //Picture
                            {
                                if (item.value == "100")
                                {
                                    var path = detail.fsource;
                                    //eliminamos la ruta
                                    detail.fsource = "";

                                    dbcmk.Entry(detail).State = EntityState.Modified;
                                    dbcmk.SaveChanges();


                                    if (System.IO.File.Exists(Server.MapPath(path)))
                                    {
                                        try
                                        {
                                            System.IO.File.Delete(Server.MapPath(path));
                                        }
                                        catch (System.IO.IOException e)
                                        {
                                            Console.WriteLine(e.Message);

                                        }
                                    }
                                }



                            }
                            else if (detail.ID_formresourcetype == 6 || detail.ID_formresourcetype == 9) //Input text y Electronic Signature
                            {

                                if (item.value == "" || item.value == null) { item.value = ""; }
                                if (detail.fsource != item.value)
                                {
                                    detail.fsource = item.value;

                                    dbcmk.Entry(detail).State = EntityState.Modified;
                                    dbcmk.SaveChanges();
                                }

                            }
                            else if (detail.ID_formresourcetype == 28) //BARCODE
                            {

                                if (item.value == "" || item.value == null) { item.value = ""; }
                                if (detail.fsource != item.value)
                                {
                                    detail.fsource = item.value;

                                    dbcmk.Entry(detail).State = EntityState.Modified;
                                    dbcmk.SaveChanges();
                                }

                            }

                            else if (detail.ID_formresourcetype == 18) //Input number
                            {

                                if (item.value == "" || item.value == null) { item.value = "0"; }
                                if (detail.fvalueDecimal != Convert.ToDecimal(item.value))
                                {
                                    detail.fvalueDecimal = Convert.ToDecimal(item.value);

                                    dbcmk.Entry(detail).State = EntityState.Modified;
                                    dbcmk.SaveChanges();
                                }

                            }
                            else if (detail.ID_formresourcetype == 21) // currency
                            {

                                if (item.value == "" || item.value == null) { item.value = "0"; }

                                if (detail.fvalueDecimal != Convert.ToDecimal(item.value))
                                {
                                    detail.fvalueDecimal = Convert.ToDecimal(item.value);

                                    dbcmk.Entry(detail).State = EntityState.Modified;
                                    dbcmk.SaveChanges();
                                }

                            }
                            else if (detail.ID_formresourcetype == 38) // Time Range
                            {

                                if (item.value == "" || item.value == null) { item.value = ""; }

                               
                                    detail.fvalueText = item.value;

                                    dbcmk.Entry(detail).State = EntityState.Modified;
                                    dbcmk.SaveChanges();
                                

                            }
                            else if (detail.ID_formresourcetype == 22) // Date
                            {

                                if (item.value == "" || item.value == null) { item.value = ""; }

                                try
                                {
                                    detail.fvalueText = Convert.ToDateTime(item.value).ToShortDateString();
                                }
                                catch
                                {
                                    detail.fvalueText = "";
                                }


                                dbcmk.Entry(detail).State = EntityState.Modified;
                                dbcmk.SaveChanges();
                                //db.Entry(detail).State = EntityState.Modified;
                                //db.SaveChanges();
                            }

                            //Select, Customer, Brands,Product line, Brand Competitors 
                            else if (detail.ID_formresourcetype == 17 || detail.ID_formresourcetype == 12 || detail.ID_formresourcetype == 13 || detail.ID_formresourcetype == 14 || detail.ID_formresourcetype == 15
                                || detail.ID_formresourcetype == 30 || detail.ID_formresourcetype == 31)
                            {
                                if (detail.fvalueText != item.value)
                                {
                                    detail.fvalueText = item.value; //Lo guardamos como texto por si colocan ID tipo cadena
                                    detail.fdescription = item.text;

                                    dbcmk.Entry(detail).State = EntityState.Modified;
                                    dbcmk.SaveChanges();
                                }

                            }

                            else if (detail.ID_formresourcetype == 19 || detail.ID_formresourcetype == 16) //checkbox,radio
                            {

                                if (item.value == "" || item.value == null) { item.value = "false"; }
                                int seleccionado = 0;
                                if (item.value == "false")
                                {
                                    seleccionado = 0;
                                }
                                else if (item.value == "true")
                                {
                                    seleccionado = 1;
                                }

                                if (detail.fvalue != seleccionado)
                                {
                                    detail.fvalue = seleccionado; //Lo guardamos como entero

                                    dbcmk.Entry(detail).State = EntityState.Modified;
                                    dbcmk.SaveChanges();
                                }


                            }
                            else
                            {
                                //No hacemos nada porque no esta registrado
                            }

                        }


                    }
                    Session["detailsForm"] = detailsForm;

                    return Json(new { Result = "Success" });
                }
                //}
                return Json(new { Result = "Warning" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "Warning" + ex.Message });
            }

        }

        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }


        [HttpPost]
        public ActionResult UploadFiles(string id,string visitareal, string idvisita, string orientation)
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }

                        Image TargetImg = Image.FromStream(file.InputStream, true, true);
                        try
                        {
                            int or = Convert.ToInt32(orientation);

                            switch (or)
                            {
                                case 1: // landscape, do nothing
                                    break;

                                case 8: // rotated 90 right
                                        // de-rotate:
                                    TargetImg.RotateFlip(rotateFlipType: RotateFlipType.Rotate270FlipNone);
                                    break;

                                case 3: // bottoms up
                                    TargetImg.RotateFlip(rotateFlipType: RotateFlipType.Rotate180FlipNone);
                                    break;

                                case 6: // rotated 90 left
                                    TargetImg.RotateFlip(rotateFlipType: RotateFlipType.Rotate90FlipNone);
                                    break;
                            }

                        }
                        catch
                        {

                        }
                        //buscamos el id del detalle

                        List<FormsM_details> detailsForm = Session["detailsForm"] as List<FormsM_details>;
                        int iddetalle = Convert.ToInt32(id);
                        int idvisitareal = Convert.ToInt32(visitareal);
                        int idactividad = Convert.ToInt32(idvisita);
                        FormsM_details detail = new FormsM_details();

                        if (detailsForm != null)
                        {
                            detail = (from d in detailsForm where (d.idkey == iddetalle && d.ID_visit == idactividad) select d).FirstOrDefault();
                        }
                        else
                        {
                            using (var dbs = new dbComerciaEntities())
                            {

                                detail = dbs.FormsM_details.Where(a => a.ID_visit == idactividad && a.idkey == iddetalle).FirstOrDefault();
                            }

                        }

                        var pathimg = detail.fsource;

                        DateTime time = DateTime.Now;



                        using (Graphics g = Graphics.FromImage(TargetImg))
                        {
                            Image imagenfinal = (Image)TargetImg.Clone();

                            int footerHeight = 35;
                            Bitmap bitmapImg = new Bitmap(imagenfinal);// Original Image
                            Bitmap bitmapComment = new Bitmap(imagenfinal.Width, footerHeight);// Footer
                            Bitmap bitmapNewImage = new Bitmap(imagenfinal.Width, imagenfinal.Height + footerHeight);//New Image
                            Graphics graphicImage = Graphics.FromImage(bitmapNewImage);
                            graphicImage.Clear(Color.White);
                            graphicImage.DrawImage(bitmapImg, new Point(0, 0));
                            graphicImage.DrawImage(bitmapComment, new Point(bitmapComment.Width, 0));
                          
                            var path = Path.Combine(Server.MapPath("~/SharedContent/images/activities"), id + "_activity_" + detail.ID_visit + "_" + time.Minute + time.Second + ".jpg");


                            var tam = file.ContentLength;

                            Image newimage;
                            //Cambiar tamano no calidad
                            if (orientation == "-1")
                            {
                                newimage = ScaleImage(bitmapNewImage, 768, 1360);
                            }
                            else
                            {
                                newimage = ScaleImage(bitmapNewImage, 1360, 768);
                            }
                            newimage.Save(path, ImageFormat.Jpeg);


                            bitmapImg.Dispose();
                            bitmapComment.Dispose();
                            bitmapNewImage.Dispose();


                        }

                        detail.fsource = "~/SharedContent/images/activities/" + id + "_activity_" + detail.ID_visit + "_" + time.Minute + time.Second + ".jpg";

                        dbcmk.Entry(detail).State = EntityState.Modified;
                        dbcmk.SaveChanges();

                        // Returns message that successfully uploaded  
                       
                    }
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        //CLASE PARA ALMACENAR OBJETOS
        public class MyObj
        {
            public string id_resource { get; set; }
            public string fsource { get; set; }
            public string fdescription { get; set; }
            public string fvalue { get; set; }
            public int idkey { get; set; }
            public int parent { get; set; }
            public IList<MyObj> children { get; set; }
        }

        public ActionResult Getformdata(string IDform)
        {
            FormsM formsM = dbcmk.FormsM.Find(Convert.ToInt32(IDform));

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = formsM.query1;

            //Deserealizamos  los datos
            JavaScriptSerializer js = new JavaScriptSerializer();
            MyObj[] details = js.Deserialize<MyObj[]>(formsM.query2);

            return Json(details, JsonRequestBehavior.AllowGet);
        }

        public int order2 = 0;
        public string nest = "";
        public void Savelist(IList<MyObj> parentList, int formid, int parent_id = 0, int order = 0)
        {
            foreach (var item in parentList)
            {
                //AUMENTAMOS EL CONTADOR PARA ORDENAMIENTO
                order2++;
                // GUARDAMOS EL DETALLE PRINCIPAL O EL NODO HIJO

                FormsM_details detalle_nuevo = new FormsM_details();

                if (parent_id != 0)
                {
                    Console.WriteLine("GUARDANDO HIJO. PADRE ID: " + parent_id + ". ID: " + item.idkey + "description: " + item.fsource);
                    //Si es nodo hijo se coloca el idkey del padre
                    detalle_nuevo.parent = parent_id;
                    item.parent = parent_id;
                }
                else
                {
                    Console.WriteLine("GUARDANDO PADRE. ID: " + item.idkey + "description: " + item.fsource);
                    //Si es nodo hijo se coloca el idkey del padre
                    detalle_nuevo.parent = 0;
                    item.parent = 0;
                }
                detalle_nuevo.ID_formresourcetype = Convert.ToInt32(item.id_resource);
                detalle_nuevo.fsource = Convert.ToString(item.fsource);
                detalle_nuevo.fvalueText = "";
                if (Convert.ToInt32(item.id_resource) == 6)
                {
                    detalle_nuevo.fsource = "";
                    detalle_nuevo.fvalueText = Convert.ToString(item.fsource);
                }

                detalle_nuevo.fdescription = Convert.ToString(item.fdescription);
                detalle_nuevo.fvalue = 0;
                detalle_nuevo.fvalueDecimal = 0;

                detalle_nuevo.ID_formM = formid;
                //colocamos 0 ya que esta seria la plantila
                detalle_nuevo.ID_visit = 0;
                //Se coloca true ya que con esto identificamos que es un item del template original
                detalle_nuevo.original = true;
                //Colocamos numero de orden
                detalle_nuevo.obj_order = order2;
                //Colocamos grupo si tiene
                detalle_nuevo.obj_group = 0;
                //Colocamos ID generado por editor
                detalle_nuevo.idkey = order2;
                detalle_nuevo.query1 = "";
                detalle_nuevo.query2 = "";
                detalle_nuevo.ID_empresa = 11;
                //Guardando por tipo de recurso
                if (Convert.ToInt32(item.id_resource) == 6) //6 es el ID de del recurso de input_text para el tipo de comentario
                                                            //Categorias
                {
                    detalle_nuevo.fvalue = Convert.ToInt32(item.fvalue);
                }

                dbcmk.FormsM_details.Add(detalle_nuevo);
                dbcmk.SaveChanges();

                //FIN NODO PADRE O HIJO
                //VERIFICAMOS SI TIENE NODOS HIJOS Y REALIZAMOS UN BUCLE A NUESTRO CONSTRUCTOR
                if (item.children != null)
                {
                    Savelist(item.children, formid, order2, order2);

                }



            }
            //Le asignamos los padres eh hijos al codigo para leerlo posteriormente
            JavaScriptSerializer js = new JavaScriptSerializer();
            nest = js.Serialize(parentList);


        }

        [ValidateAntiForgeryToken]
        public ActionResult DeleteForm(string idFormD)
        {
            try
            {
                int IDform = Convert.ToInt32(idFormD);

                FormsM form = dbcmk.FormsM.Find(IDform);
                dbcmk.FormsM.Remove(form);
                dbcmk.SaveChanges();

                var detalle = (from d in dbcmk.FormsM_details where (d.ID_formM == IDform && d.original == true) select d).ToList();
                foreach (var det in detalle)
                {
                    FormsM_details detf = dbcmk.FormsM_details.Find(det.ID_details);
                    dbcmk.FormsM_details.Remove(detf);
                    dbcmk.SaveChanges();

                }


                //TempData["exito"] = "Form deleted successfully.";
                return RedirectToAction("FormsM", "Management", null);
            }
            catch (Exception ex)
            {
                //TempData["advertencia"] = "Something wrong happened, try again. " + ex.Message;
                return RedirectToAction("FormsM", "Management", null);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateForm([Bind(Include = "ID_form,name,description, ID_activity,active")] FormsM formsM, string nestable_output, string action, string possibleID)
        {


            formsM.active = true;
            formsM.query1 = nestable_output;
            formsM.query2 = "";
            formsM.ID_empresa = 11; // codigo de LIMENA EN COMERCIA WEB APP
            if (ModelState.IsValid)
            {
                //Deserealizamos  los datos
                JavaScriptSerializer js = new JavaScriptSerializer();
                MyObj[] details = js.Deserialize<MyObj[]>(nestable_output);



                int idfinal = 0;

                if (details.Count() > 0)
                {
                    ////Guardamos formulario
                    //Determinamos si es un update o un create
                    if (action == "update")
                    {
                        FormsM formsMi = dbcmk.FormsM.Find(Convert.ToInt32(possibleID));

                        formsMi.name = formsM.name;
                        formsMi.description = formsM.description;
                        formsMi.ID_activity = formsM.ID_activity;
                        formsMi.query1 = nestable_output;
                        formsMi.query2 = "";
                        formsMi.ID_empresa = formsM.ID_empresa;


                        dbcmk.Entry(formsMi).State = EntityState.Modified;
                        dbcmk.SaveChanges();


                        idfinal = formsMi.ID_form;

                        //Eliminamos el detalle
                        var detalles = (from t in dbcmk.FormsM_details where (t.ID_formM == formsMi.ID_form && t.original == true) select t).ToList();

                        foreach (var detalle in detalles)
                        {
                            FormsM_details formsMDet = dbcmk.FormsM_details.Find(detalle.ID_details);
                            dbcmk.FormsM_details.Remove(formsMDet);
                            dbcmk.SaveChanges();
                        }


                    }
                    else if (action == "create")
                    {
                        dbcmk.FormsM.Add(formsM);
                        dbcmk.SaveChanges();

                        idfinal = formsM.ID_form;
                    }


                    ////Guardamos detalle de formulario con Original = true
                    //Llamamos el constructor el cual consiste en un bucle "infinito" en base a los nodos padre eh hijos que contenta el array de objetos del editor
                    //del formulario
                    order2 = 0;

                    if (formsM.ID_activity == 2)
                    {
                        Savelist_retail(details, idfinal);
                    }
                    else
                    {
                        Savelist(details, idfinal);
                    }




                    FormsM formsMlast = dbcmk.FormsM.Find(idfinal);

                    formsMlast.query2 = nest;

                    dbcmk.Entry(formsMlast).State = EntityState.Modified;
                    dbcmk.SaveChanges();

                    ////***********************************************

                    //TempData["exito"] = "Form created successfully.";
                    return RedirectToAction("FormsM", "Management", null);
                }
                else
                {
                    //TempData["advertencia"] = "No data found, try again.";
                    return RedirectToAction("FormsM", "Management", null);
                }


            }


            //TempData["advertencia"] = "Something wrong happened, try again.";
            return RedirectToAction("FormsM", "Home", null);
        }

        public void Savelist_retail(IList<MyObj> parentList, int formid, int parent_id = 0, int order = 0)
        {
            var mainlist = parentList;
            var listaColumnas = (from a in parentList where (a.id_resource == "24") select a).ToList();
            foreach (var item in parentList)
            {
                //AUMENTAMOS EL CONTADOR PARA ORDENAMIENTO
                order2++;
                // GUARDAMOS EL DETALLE PRINCIPAL O EL NODO HIJO

                FormsM_details detalle_nuevo = new FormsM_details();

                detalle_nuevo.parent = 0;
                detalle_nuevo.ID_formresourcetype = Convert.ToInt32(item.id_resource);
                detalle_nuevo.fsource = Convert.ToString(item.fsource);
                detalle_nuevo.fvalueText = "";
                if (Convert.ToInt32(item.id_resource) == 6)
                {
                    detalle_nuevo.fsource = "";
                    detalle_nuevo.fvalueText = Convert.ToString(item.fsource);
                }

                detalle_nuevo.fdescription = Convert.ToString(item.fdescription);
                detalle_nuevo.fvalue = 0;
                detalle_nuevo.fvalueDecimal = 0;

                detalle_nuevo.ID_formM = formid;
                //colocamos 0 ya que esta seria la plantila
                detalle_nuevo.ID_visit = 0;
                //Se coloca true ya que con esto identificamos que es un item del template original
                detalle_nuevo.original = true;
                //Colocamos numero de orden
                detalle_nuevo.obj_order = order2;
                //Colocamos grupo si tiene
                detalle_nuevo.obj_group = 0;
                //Colocamos ID generado por editor
                detalle_nuevo.idkey = order2;
                detalle_nuevo.query1 = "";
                detalle_nuevo.query2 = "";
                detalle_nuevo.ID_empresa = 11;
                //Guardando por tipo de recurso
                if (Convert.ToInt32(item.id_resource) == 6) //6 es el ID de del recurso de input_text para el tipo de comentario
                                                            //Categorias
                {
                    detalle_nuevo.fvalue = Convert.ToInt32(item.fvalue);
                }
                if (Convert.ToInt32(item.id_resource) == 24) //24 es el ID de del recurso de Column para el tipo de datos
                                                             //Tipo de datos
                {
                    detalle_nuevo.fvalue = Convert.ToInt32(item.fvalue);
                }
                dbcmk.FormsM_details.Add(detalle_nuevo);
                dbcmk.SaveChanges();

                if (Convert.ToInt32(item.id_resource) == 3)
                { //PARA PRODUCTO
                    foreach (var itemColumna in listaColumnas)
                    {
                        //AUMENTAMOS EL CONTADOR PARA ORDENAMIENTO
                        order2++;
                        if (itemColumna.fvalue == "16")
                        {
                            //Multiple choice
                            FormsM_details Subdetalle_nuevo = new FormsM_details();

                            Subdetalle_nuevo.parent = detalle_nuevo.idkey;
                            Subdetalle_nuevo.ID_formresourcetype = 16;
                            Subdetalle_nuevo.fsource = "";
                            Subdetalle_nuevo.fvalueText = "";

                            Subdetalle_nuevo.fdescription = Convert.ToString(itemColumna.fdescription);
                            Subdetalle_nuevo.fvalue = 0;
                            Subdetalle_nuevo.fvalueDecimal = 0;

                            Subdetalle_nuevo.ID_formM = formid;
                            //colocamos 0 ya que esta seria la plantila
                            Subdetalle_nuevo.ID_visit = 0;
                            //Se coloca true ya que con esto identificamos que es un item del template original
                            Subdetalle_nuevo.original = true;
                            //Colocamos numero de orden
                            Subdetalle_nuevo.obj_order = order2;
                            //Colocamos grupo si tiene
                            Subdetalle_nuevo.obj_group = 0;
                            //Colocamos ID generado por editor
                            Subdetalle_nuevo.idkey = order2;
                            Subdetalle_nuevo.query1 = "";
                            Subdetalle_nuevo.query2 = "";
                            Subdetalle_nuevo.ID_empresa = 11;

                            dbcmk.FormsM_details.Add(Subdetalle_nuevo);
                            dbcmk.SaveChanges();
                        }
                        else if (itemColumna.fvalue == "21")
                        {
                            //Currency
                            FormsM_details Subdetalle_nuevo = new FormsM_details();

                            Subdetalle_nuevo.parent = detalle_nuevo.idkey;
                            Subdetalle_nuevo.ID_formresourcetype = 21;
                            Subdetalle_nuevo.fsource = "";
                            Subdetalle_nuevo.fvalueText = "";

                            Subdetalle_nuevo.fdescription = Convert.ToString(itemColumna.fdescription);
                            Subdetalle_nuevo.fvalue = 0;
                            Subdetalle_nuevo.fvalueDecimal = 0;

                            Subdetalle_nuevo.ID_formM = formid;
                            //colocamos 0 ya que esta seria la plantila
                            Subdetalle_nuevo.ID_visit = 0;
                            //Se coloca true ya que con esto identificamos que es un item del template original
                            Subdetalle_nuevo.original = true;
                            //Colocamos numero de orden
                            Subdetalle_nuevo.obj_order = order2;
                            //Colocamos grupo si tiene
                            Subdetalle_nuevo.obj_group = 0;
                            //Colocamos ID generado por editor
                            Subdetalle_nuevo.idkey = order2;
                            Subdetalle_nuevo.query1 = "";
                            Subdetalle_nuevo.query2 = "";
                            Subdetalle_nuevo.ID_empresa = 11;

                            dbcmk.FormsM_details.Add(Subdetalle_nuevo);
                            dbcmk.SaveChanges();
                        }
                    }

                }


            }
            //Le asignamos los padres eh hijos al codigo para leerlo posteriormente
            JavaScriptSerializer js = new JavaScriptSerializer();
            nest = js.Serialize(parentList);


        }

        //FUNCIONANDO 01/17/2020
        //[HttpPost]
        //public ActionResult UploadFiles(string id, string idvisita, string orientation)
        //{
        //    List<FormsM_details> detailsForm = Session["detailsForm"] as List<FormsM_details>;
        //    int act = Convert.ToInt32(id);
        //    if (detailsForm != null)
        //    {

        //    }
        //    else
        //    {
        //        using (var dbs = new dbComerciaEntities())
        //        {

        //            detailsForm = dbs.FormsM_details.Where(a => a.ID_visit == act).ToList();
        //        }

        //    }

        //    // Checking no of files injected in Request object  
        //    if (Request.Files.Count > 0)
        //    {
        //        try
        //        {
        //            //  Get all files from Request object  
        //            HttpFileCollectionBase files = Request.Files;
        //            for (int i = 0; i < files.Count; i++)
        //            {
        //                //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
        //                //string filename = Path.GetFileName(Request.Files[i].FileName);  

        //                HttpPostedFileBase file = files[i];
        //                string fname;

        //                // Checking for Internet Explorer  
        //                if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
        //                {
        //                    string[] testfiles = file.FileName.Split(new char[] { '\\' });
        //                    fname = testfiles[testfiles.Length - 1];
        //                }
        //                else
        //                {
        //                    fname = file.FileName;
        //                }


        //                // Adding watermark to the image and saving it into the specified folder!!!!

        //                //Image image = Image.FromStream(file.InputStream, true, true);


        //                Image TargetImg = Image.FromStream(file.InputStream, true, true);
        //                try
        //                {
        //                    int or = Convert.ToInt32(orientation);

        //                    switch (or)
        //                    {
        //                        case 1: // landscape, do nothing
        //                            break;

        //                        case 8: // rotated 90 right
        //                                // de-rotate:
        //                            TargetImg.RotateFlip(rotateFlipType: RotateFlipType.Rotate270FlipNone);
        //                            break;

        //                        case 3: // bottoms up
        //                            TargetImg.RotateFlip(rotateFlipType: RotateFlipType.Rotate180FlipNone);
        //                            break;

        //                        case 6: // rotated 90 left
        //                            TargetImg.RotateFlip(rotateFlipType: RotateFlipType.Rotate90FlipNone);
        //                            break;
        //                    }

        //                }
        //                catch
        //                {

        //                }
        //                //buscamos el id del detalle
        //                int idf = Convert.ToInt32(id);
        //                FormsM_details detail = new FormsM_details();
        //                FormsM_details detailBrand = new FormsM_details();

        //                try
        //                {
        //                    int idvisit = Convert.ToInt32(idvisita);
        //                    detail = (from d in detailsForm where (d.idkey == idf && d.ID_visit == idvisit) select d).FirstOrDefault();
        //                }
        //                catch
        //                {
        //                    var sqlQueryText = string.Format("SELECT * FROM FormsM_details WHERE query2 LIKE '{0}' and idkey='" + idf + "'", idvisita);
        //                    detail = dbcmk.FormsM_details.SqlQuery(sqlQueryText).FirstOrDefault(); //returns 0 or more rows satisfying sql query

        //                }
        //                try
        //                {
        //                    int idvisit2 = Convert.ToInt32(idvisita);
        //                    detailBrand = (from d in detailsForm where (d.ID_visit == idvisit2 && d.ID_formresourcetype == 13) select d).FirstOrDefault();
        //                }
        //                catch { }

        //                var pathimg = detail.fsource;

        //                DateTime time = DateTime.Now;

        //                var footer = (from a in dbcmk.ActivitiesM where (a.ID_activity == detail.ID_visit) select a).FirstOrDefault();

        //                var customer = "";
        //                var date = "";
        //                var activi = "";
        //                var store = "";
        //                var brand = "";
        //                if (detailBrand != null)
        //                {
        //                    brand = detailBrand.fdescription;
        //                }

        //                if (footer != null)
        //                {
        //                    var visit = (from a in dbcmk.VisitsM where (a.ID_visit == footer.ID_visit) select a).FirstOrDefault();
        //                    if (visit != null)
        //                    {
        //                        store = visit.store + ", " + visit.address + ", " + visit.city + ", " + visit.state;
        //                    }

        //                    customer = footer.ID_customer + "-" + footer.Customer;
        //                    date = visit.visit_date.ToShortDateString();
        //                    activi = footer.ID_activity + "-" + footer.description;
        //                }


        //                using (Image watermark = Image.FromFile(Server.MapPath("~/Content/images/Logo_watermark.png")))
        //                using (Graphics g = Graphics.FromImage(TargetImg))
        //                {

        //                    Image thumb = watermark.GetThumbnailImage((TargetImg.Width / 2), (TargetImg.Height / 3), null, IntPtr.Zero);

        //                    var destX = (TargetImg.Width / 2 - thumb.Width / 2);
        //                    var destY = (TargetImg.Height / 2 - thumb.Height / 2);

        //                    g.DrawImage(watermark, new Rectangle(destX,
        //                                destY,
        //                                TargetImg.Width / 2,
        //                                TargetImg.Height / 4));


        //                    // display a clone for demo purposes
        //                    //pb2.Image = (Image)TargetImg.Clone();
        //                    Image imagenfinal = (Image)TargetImg.Clone();

        //                    int footerHeight = 35;
        //                    Bitmap bitmapImg = new Bitmap(imagenfinal);// Original Image
        //                    Bitmap bitmapComment = new Bitmap(imagenfinal.Width, footerHeight);// Footer
        //                    Bitmap bitmapNewImage = new Bitmap(imagenfinal.Width, imagenfinal.Height + footerHeight);//New Image
        //                    Graphics graphicImage = Graphics.FromImage(bitmapNewImage);
        //                    graphicImage.Clear(Color.White);
        //                    graphicImage.DrawImage(bitmapImg, new Point(0, 0));
        //                    graphicImage.DrawImage(bitmapComment, new Point(bitmapComment.Width, 0));
        //                    graphicImage.DrawString((store + " | " + brand + " | " + date + " | " + activi), new Font("Arial", 19), new SolidBrush(Color.Black), 0, bitmapImg.Height + footerHeight / 6);



        //                    var path = Path.Combine(Server.MapPath("~/SharedContent/images/activities"), id + "_activity_" + detail.ID_visit + "_" + time.Minute + time.Second + ".jpg");


        //                    var tam = file.ContentLength;

        //                    //if (tam < 600000)
        //                    //{
        //                    //bitmapNewImage.Save(path, ImageFormat.Jpeg);
        //                    Image newimage;
        //                    //Cambiar tamano no calidad
        //                    if (orientation == "-1")
        //                    {
        //                        newimage = ScaleImage(bitmapNewImage, 768, 1360);
        //                    }
        //                    else
        //                    {
        //                        newimage = ScaleImage(bitmapNewImage, 1360, 768);
        //                    }
        //                    newimage.Save(path, ImageFormat.Jpeg);


        //                    bitmapImg.Dispose();
        //                    bitmapComment.Dispose();
        //                    bitmapNewImage.Dispose();


        //                }


        //                //fname = Path.Combine(Server.MapPath("~/Content/images/ftp_demo"), fname);
        //                //file.SaveAs(fname);

        //                //Luego guardamos la url en la db
        //                //Forms_details detail = dbcmk.Forms_details.Find(Convert.ToInt32(id));  //se movio hacia arriba
        //                detail.fsource = "~/SharedContent/images/activities/" + id + "_activity_" + detail.ID_visit + "_" + time.Minute + time.Second + ".jpg";

        //                dbcmk.Entry(detail).State = EntityState.Modified;
        //                dbcmk.SaveChanges();

        //                if (System.IO.File.Exists(Server.MapPath(pathimg)))
        //                {
        //                    try
        //                    {
        //                        System.IO.File.Delete(Server.MapPath(pathimg));
        //                    }
        //                    catch (System.IO.IOException e)
        //                    {
        //                        Console.WriteLine(e.Message);

        //                    }
        //                }
        //            }


        //            // Returns message that successfully uploaded  
        //            return Json("File Uploaded Successfully!");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json("Error occurred.");
        //        }
        //    }
        //    else
        //    {
        //        return Json("No files selected.");
        //    }
        //}

    }
}