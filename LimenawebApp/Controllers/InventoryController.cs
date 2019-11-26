using LimenawebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace LimenawebApp.Controllers
{
    public class InventoryController : Controller
    {
        private dbLimenaEntities dblim = new dbLimenaEntities();
        private DLI_PROEntities dlipro = new DLI_PROEntities();
        private Interna_DLIEntities internadli = new Interna_DLIEntities();
        private dbComerciaEntities dbcmk = new dbComerciaEntities();
        //CLASS GENERAL
        private clsGeneral generalClass = new clsGeneral();


        public bool isAdminRole (string roles) {
            //SABER SI ES ADMIN
            bool isAdmin = false;
            if (roles.Contains("Super Admin") || roles.Contains("Admin") || roles.Contains("Supervisor"))
            {
                isAdmin = true;
            }
            else
            {
                isAdmin = false;

            }
            return isAdmin;
        }


        // GET: Inventory
        public ActionResult Projects()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Inventory";
                ViewData["Page"] = "Projects";
                ViewBag.menunameid = "inventory_menu";
                ViewBag.submenunameid = "stock_submenu";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //FIN HEADER

                var adminandpicker = 0;
                var isadmin = isAdminRole(activeuser.Roles);
                var issuperadmin = 0;
                int cuenta = 0;
                List<Inv_Projects> lstprojects = new List<Inv_Projects>();
                if (isadmin)
                {
                    if (activeuser.Roles.Contains("Picker"))
                    {
                        adminandpicker = 1;
                    }
                    if (activeuser.Roles.Contains("Admin"))
                    {
                        issuperadmin = 1;
                    }
                    lstprojects = dblim.Inv_Projects.ToList();
                }
                else {
             
                    var binloc = dblim.Inv_Projects_BinLoc.Where(c => c.ID_userAssigned== activeuser.ID_User).ToList();

                    var projects = binloc.Select(c => c.ID_project).Distinct().ToArray();

                    lstprojects = dblim.Inv_Projects.Where(c => projects.Contains(c.ID_project)).ToList();
                    
                }
                ViewBag.isadminandticker = adminandpicker;
                ViewBag.cuenta = cuenta;
                ViewBag.adminRole = isadmin;
                ViewBag.issuperadmin = issuperadmin;
                return View(lstprojects);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }



        public ActionResult DeleteProject(int id)

        {
            try
            {            
                var binloc = dblim.Inv_Projects_BinLoc.Where(c => c.ID_project == id).Select(c => c.ID_binloc).ToArray();
                var delete = dblim.Inv_Projects_Task.Where(c => binloc.Contains(c.ID_binloc)).ToList();

                dblim.BulkDelete(delete);

                dblim.Database.ExecuteSqlCommand("delete from Inv_Projects_BinLoc where ID_project={0}", id);
                dblim.Database.ExecuteSqlCommand("delete from Inv_Projects where ID_project={0}", id);
                return RedirectToAction("Projects", "Inventory", null);
            }
            catch
            {

                return RedirectToAction("Projects", "Inventory", null);
            }



        }
        public class binlocationslist

        {
            public int id { get; set; }
            public string BinCode { get; set; }
            public string WhsCode { get; set; }
            public string Aisle { get; set; }
            public string Position { get; set; }
            public string Level { get; set; }
            public string PalletPosition { get; set; }
            public string Type { get; set; }
            public string Area { get; set; }
            public string Order { get; set; }

        }
        public ActionResult Projects_new()
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Inventory";
                ViewData["Page"] = "Projects";
                ViewBag.menunameid = "inventory_menu";
                ViewBag.submenunameid = "stock_submenu";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //FIN HEADER

                var lstBinLocations = dlipro.OBIN.Where(a => !a.SL1Code.Contains("SYSTEM-BIN-LOCATION") && !a.SL1Code.Contains("ST")).Select(c=> new binlocationslist { id=c.AbsEntry, BinCode=c.BinCode, WhsCode=c.WhsCode, Aisle=c.SL1Code,
                Position=c.SL2Code,Level=c.SL3Code, PalletPosition=c.SL4Code, Type=c.Attr1Val, Area=c.Attr2Val, Order=c.Attr3Val});

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                ViewBag.arrayBin = javaScriptSerializer.Serialize(lstBinLocations);

                var usuarios = dblim.Sys_Users.Where(c => c.ID_Company == 1 && c.Roles.Contains("Picker"));
                ViewBag.usuarios = usuarios;
                return View(lstBinLocations);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        [HttpPost]
        public ActionResult Save_ProjectDetails(List<Inv_Projects_BinLoc> objects, string projectName, string ProjectDes, int maxcounts)
        {
            string ttresult = "";
            try
            {
                if (generalClass.checkSession())
                {
                    Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                    var usuario = (from a in dblim.Sys_Users where (a.ID_User == activeuser.ID_User) select a).FirstOrDefault();

                    //Creamos proyecto
                    Inv_Projects new_project = new Inv_Projects();
                    new_project.Name = projectName;
                    new_project.Description = ProjectDes;
                    new_project.Max_counts = maxcounts;
                    new_project.Creation_date = DateTime.UtcNow;
                    new_project.End_date = DateTime.UtcNow;
                    new_project.IDusers_assigned = "";
                    new_project.Areas = 0;
                    IList<string> stringsIds = new List<string>();
                    stringsIds.Add(activeuser.ID_User.ToString());
                    if (objects != null && objects.Count > 0)
                    {
                        new_project.Areas = objects.Count();
                        var diffids = objects.Select(c => c.ID_userAssigned).Distinct().ToList();
                        if (diffids.Count > 0)
                        {
                            foreach (var iditem in diffids)
                            {
                                stringsIds.Add(iditem.ToString());
                            }
                            string joined = string.Join(",", stringsIds.ToArray());
                            new_project.IDusers_assigned = joined;
                        }
                    }
                    else {
                        new_project.IDusers_assigned = activeuser.ID_User.ToString();
                    }

                        
                    new_project.isfinished = false;
                    new_project.isuploaded = false;
                    
                    new_project.Error = 0;
                    new_project.MensajeError = "";
                    new_project.ID_empresa = activeuser.ID_Company;

                    dblim.Inv_Projects.Add(new_project);
                    dblim.SaveChanges();



                    if (objects != null && objects.Count > 0)
                    {

                        foreach (var items in objects)
                        {
                            items.Count = maxcounts;//max conteo
                            if (items.Area == null) { items.Area = ""; }
                            if (items.Type == null) { items.Type = ""; }
                            items.creation_date = DateTime.UtcNow;
                            items.ID_userCreate = activeuser.ID_User;
                            items.UserNameCreate = activeuser.Name.ToUpper() + " " + activeuser.Lastname.ToUpper();
                            items.end_date = DateTime.UtcNow;
                            items.Actual_counts = 1; //conteo por defecto
                            items.ID_status = 0;
                            items.isfinished = false;
                            items.comments = "";
                            items.ID_project = new_project.ID_project;
                            items.ID_empresa = activeuser.ID_Company;
                            items.Error = false;
                            items.MensajeError = "";
                            items.OnSAP = false;
                        }
                        try
                        {
                            dblim.BulkInsert(objects);

                            List<Inv_Projects_Task> lsttoadd = new List<Inv_Projects_Task>();
                            foreach (var items in objects)
                            {
                                Inv_Projects_Task newtask = new Inv_Projects_Task();
                                newtask.ID_binloc = items.ID_binloc;
                                newtask.Bin_location = items.Bin_location;
                                newtask.comments = "";
                                newtask.creation_date = DateTime.UtcNow;
                                newtask.end_date = DateTime.UtcNow;
                                newtask.ID_userEnd = items.ID_userAssigned;
                                newtask.UserName = items.UserNameAssigned;
                                newtask.ID_status = 1;
                                newtask.Count = 1;
                                newtask.isselected = false;
                                newtask.ItemCode = "";
                                newtask.ItemName = "";
                                newtask.UoM_code = "";
                                newtask.UoM_entry = 0;
                                newtask.Quantity = 0;
                                newtask.UoM_code2 = "";
                                newtask.UoM_entry2 = 0;
                                newtask.Quantity2 = 0;
                                newtask.UoM_code3 = "";
                                newtask.UoM_entry3 = 0;
                                newtask.Quantity3 = 0;
                                newtask.UoM_code4 = "";
                                newtask.UoM_entry4 = 0;
                                newtask.Quantity4 = 0;
                                newtask.Final_quantity = 0;
                                newtask.ID_empresa = activeuser.ID_Company;
                                newtask.ID_project = new_project.ID_project;
                                newtask.Internal_sort = items.Internal_sort;
                                newtask.Type = items.Type;
                                newtask.Aisle=items.Aisle;
                                newtask.Area = items.Area;
                                lsttoadd.Add(newtask);
                            }



                            dblim.BulkInsert(lsttoadd);


                        }
                        catch (Exception ex)
                        {
                            ttresult = "ERROR SAVING DATA: " + ex.Message;
                            return Json(ttresult, JsonRequestBehavior.AllowGet);
                        }


                    }

                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }
                ttresult = "Expired session, please refresh the page.";
                return Json(ttresult, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }

        public class taskTodelete

        {
            public int idtask { get; set; }

        }
        [HttpPost]
        public ActionResult Delete_Details(List<taskTodelete> objects, int maxcounts, int idproj)
        {
            string ttresult = "";
            try
            {


                        try
                        {

                        var arr = objects.Select(c => c.idtask).ToArray();

                        var listtodelete = dblim.Inv_Projects_Task.Where(c => arr.Contains(c.ID_projects_task)).ToList();
                        dblim.BulkDelete(listtodelete);



                        }
                        catch (Exception ex)
                        {
                            ttresult = "ERROR SAVING DATA: " + ex.Message;
                            return Json(ttresult, JsonRequestBehavior.AllowGet);
                        }



                    

                ttresult = "SUCCESS";
                return Json(ttresult, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }

        [HttpPost]
        public ActionResult Save_Details(List<Inv_Projects_BinLoc> objects, int maxcounts, int idproj)
        {
            string ttresult = "";
            try
            {
                if (generalClass.checkSession())
                {
                    Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                    var usuario = (from a in dblim.Sys_Users where (a.ID_User == activeuser.ID_User) select a).FirstOrDefault();
                    var project = dblim.Inv_Projects.Where(f => f.ID_project == idproj).FirstOrDefault();

                    if (objects != null && objects.Count > 0)
                    {

                        //Creamos uno nuevo
                        List<Inv_Projects_BinLoc> existen = new List<Inv_Projects_BinLoc>();
                        foreach (var items in objects)
                        {
                            var itemexist = dblim.Inv_Projects_BinLoc.Where(d => d.ID_project == idproj && d.Bin_location == items.Bin_location).FirstOrDefault();

                            if (itemexist != null) //si se esta en el mismo conteo NUNCA aparecera, mpero si es un nuevo conteo aparecera, por lo tanto hay que borrar esta cabecera
                            {
                                items.ID_binloc = itemexist.ID_binloc;
                                existen.Add(items);

                            }
                            else
                            {

                                items.Count = project.Max_counts;//max conteo
                                if (items.Area == null) { items.Area = ""; }
                                if (items.Type == null) { items.Type = ""; }
                                items.creation_date = DateTime.UtcNow;
                                items.ID_userCreate = activeuser.ID_User;
                                items.UserNameCreate = activeuser.Name.ToUpper() + " " + activeuser.Lastname.ToUpper();
                                items.end_date = DateTime.UtcNow;
                                items.Actual_counts = maxcounts; //conteo por defecto

                                items.ID_status = 0;
                                items.isfinished = false;
                                items.comments = "";
                                items.ID_project = idproj;
                                items.ID_empresa = activeuser.ID_Company;
                                items.Error = false;
                                items.MensajeError = "";
                                items.OnSAP = false;
                            }


                        }
                        foreach (var ext in existen)
                        {
                            objects.Remove(ext);
                        }
                        try
                        {
                            dblim.BulkInsert(objects);

                            List<Inv_Projects_Task> lsttoadd = new List<Inv_Projects_Task>();
                            foreach (var items in objects)
                            {
                                Inv_Projects_Task newtask = new Inv_Projects_Task();
                                newtask.ID_binloc = items.ID_binloc;
                                newtask.Bin_location = items.Bin_location;
                                newtask.comments = "";
                                newtask.creation_date = DateTime.UtcNow;
                                newtask.end_date = DateTime.UtcNow;
                                newtask.ID_userEnd = items.ID_userAssigned;
                                newtask.UserName = items.UserNameAssigned;
                                newtask.ID_status = 1;
                                newtask.Count = maxcounts;
                                newtask.isselected = false;
                                newtask.ItemCode = "";
                                newtask.ItemName = "";
                                newtask.UoM_code = "";
                                newtask.UoM_entry = 0;
                                newtask.Quantity = 0;
                                newtask.UoM_code2 = "";
                                newtask.UoM_entry2 = 0;
                                newtask.Quantity2 = 0;
                                newtask.UoM_code3 = "";
                                newtask.UoM_entry3 = 0;
                                newtask.Quantity3 = 0;
                                newtask.UoM_code4 = "";
                                newtask.UoM_entry4 = 0;
                                newtask.Quantity4 = 0;
                                newtask.Final_quantity = 0;
                                newtask.ID_empresa = activeuser.ID_Company;
                                newtask.ID_project = idproj;
                                newtask.Internal_sort = items.Internal_sort;
                                newtask.Area = items.Area;
                                newtask.Type = items.Type;
                                newtask.Aisle = items.Aisle;
                                lsttoadd.Add(newtask);
                            }



                            dblim.BulkInsert(lsttoadd);


                            List<Inv_Projects_Task> lsttoadd2 = new List<Inv_Projects_Task>();
                            foreach (var items in existen)
                            {
                                Inv_Projects_Task newtask2 = new Inv_Projects_Task();
                                newtask2.ID_binloc = items.ID_binloc;
                                newtask2.Bin_location = items.Bin_location;
                                newtask2.comments = "";
                                newtask2.creation_date = DateTime.UtcNow;
                                newtask2.end_date = DateTime.UtcNow;
                                newtask2.ID_userEnd = items.ID_userAssigned;
                                newtask2.UserName = items.UserNameAssigned;
                                newtask2.ID_status = 1;
                                newtask2.Count = maxcounts;
                                newtask2.isselected = false;
                                newtask2.ItemCode = "";
                                newtask2.ItemName = "";
                                newtask2.UoM_code = "";
                                newtask2.UoM_entry = 0;
                                newtask2.Quantity = 0;
                                newtask2.UoM_code2 = "";
                                newtask2.UoM_entry2 = 0;
                                newtask2.Quantity2 = 0;
                                newtask2.UoM_code3 = "";
                                newtask2.UoM_entry3 = 0;
                                newtask2.Quantity3 = 0;
                                newtask2.UoM_code4 = "";
                                newtask2.UoM_entry4 = 0;
                                newtask2.Quantity4 = 0;
                                newtask2.Final_quantity = 0;
                                newtask2.ID_empresa = activeuser.ID_Company;
                                newtask2.ID_project = idproj;
                                newtask2.Internal_sort = items.Internal_sort;
                                newtask2.Area = items.Area;
                                newtask2.Type = items.Type;
                                newtask2.Aisle = items.Aisle;
                                lsttoadd2.Add(newtask2);
                            }



                            dblim.BulkInsert(lsttoadd2);


                        }
                        catch (Exception ex)
                        {
                            ttresult = "ERROR SAVING DATA: " + ex.Message;
                            return Json(ttresult, JsonRequestBehavior.AllowGet);
                        }

                        project.Areas = project.Areas + objects.Count();
                        dblim.Entry(project).State = EntityState.Modified;
                        dblim.SaveChanges();

                    }

                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }
                ttresult = "Expired session, please refresh the page.";
                return Json(ttresult, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }




        [HttpPost]
        public ActionResult SaveProduct(string ItemCode, string ItemName, int ID_Picker, string pickerName, int idproj, int actualcount)
        {
            string ttresult = "";
            try
            {
                if (generalClass.checkSession())
                {
                    Sys_Users activeuser = Session["activeUser"] as Sys_Users;

                    var usuario = (from a in dblim.Sys_Users where (a.ID_User == activeuser.ID_User) select a).FirstOrDefault();
                    var project = dblim.Inv_Projects.Where(f => f.ID_project == idproj).FirstOrDefault();


                        dblim.Entry(project).State = EntityState.Modified;
                        dblim.SaveChanges();

                    

                    ttresult = "SUCCESS";
                    return Json(ttresult, JsonRequestBehavior.AllowGet);
                }
                ttresult = "Expired session, please refresh the page.";
                return Json(ttresult, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ttresult = "ERROR: " + ex.Message;
                return Json(ttresult, JsonRequestBehavior.AllowGet);
            }




        }



        public ActionResult Binloc_master(int idproj, int count)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Inventory";
                ViewData["Page"] = "Projects";
                ViewBag.menunameid = "inventory_menu";
                ViewBag.submenunameid = "stock_submenu";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //FIN HEADER

                var binloctohide = dblim.Inv_Projects_Task.Where(c => c.ID_project == idproj && c.Count == count).Select(c=>c.Bin_location).ToArray();

                var lstBinLocations = dlipro.OBIN.Where(a => !a.SL1Code.Contains("SYSTEM-BIN-LOCATION") && !a.SL1Code.Contains("ST") && !binloctohide.Contains(a.BinCode)).Select(c => new binlocationslist
                {
                    id = c.AbsEntry,
                    BinCode = c.BinCode,
                    WhsCode = c.WhsCode,
                    Aisle = c.SL1Code,
                    Position = c.SL2Code,
                    Level = c.SL3Code,
                    PalletPosition = c.SL4Code,
                    Type = c.Attr1Val,
                    Area = c.Attr2Val,
                    Order = c.Attr3Val
                });

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                ViewBag.arrayBin = javaScriptSerializer.Serialize(lstBinLocations);

                var usuarios = dblim.Sys_Users.Where(c => c.ID_Company == 1 && c.Roles.Contains("Picker"));
                ViewBag.usuarios = usuarios;

                var project = dblim.Inv_Projects.Where(f => f.ID_project == idproj).FirstOrDefault();
                ViewBag.project = project;

                var history = dblim.Inv_Projects_Task.Where(c => binloctohide.Contains(c.Bin_location) && c.Count == count && c.ID_project==idproj).ToList();
                ViewBag.history = history;

                ViewBag.count = count;

                var generalProducts = (from a in dlipro.BI_Dim_Products select a).ToList();
                ViewBag.generalProducts = generalProducts;

                return View(lstBinLocations);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
        public ActionResult BinLoc_select0count(int idproj, int selectedcount)
        {
            try
            {
                //Reiniciamos el estado de los bin location seleccionados para comprobar nuevamente
                dblim.Database.ExecuteSqlCommand("update Inv_Projects_Task set isselected=0 where ID_project={0}", idproj);
            }
            catch {
                return RedirectToAction("Binloc_general", "Inventory", new { idproj = idproj, msj=2 });
            }

            return RedirectToAction("Binloc_general", "Inventory", new { idproj = idproj, msj = 1 });


        }
        public ActionResult selectBinCount(int idproj, int idtask, int idbin, bool selected)
        {
            try
            {
                //Reiniciamos el estado de los bin location relacionados
                dblim.Database.ExecuteSqlCommand("update Inv_Projects_Task set isselected=0 where ID_project={0} and ID_binloc={1}", idproj, idbin);
                //Actualizamos el que nos interesa en base al valor que envia el usuario
                dblim.Database.ExecuteSqlCommand("update Inv_Projects_Task set isselected={0} where ID_project={1} and ID_binloc={2} and ID_projects_task={3}", selected, idproj, idbin, idtask);
            }
            catch {
                return Json("error", JsonRequestBehavior.AllowGet);
            }

            return Json("success", JsonRequestBehavior.AllowGet);


        }

        public ActionResult BinLoc_selectcount(int idproj, int selectedcount)
        {
            try
            {
                //Reiniciamos el estado de los bin location seleccionados para comprobar nuevamente
                dblim.Database.ExecuteSqlCommand("update Inv_Projects_Task set isselected=0 where ID_project={0}", idproj);
                dblim.Database.ExecuteSqlCommand("update Inv_Projects_Task set isselected=1 where ID_project={0} and Count={1} and ItemCode<>'' and ItemCode<>'0'", idproj, selectedcount);
            }
            catch
            {
                return RedirectToAction("Binloc_general", "Inventory", new { idproj = idproj, msj = 2 });
            }

            return RedirectToAction("Binloc_general", "Inventory", new { idproj = idproj, msj = 1 });


        }

        public ActionResult Binloc_general(int idproj, int msj=0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Inventory";
                ViewData["Page"] = "Projects";
                ViewBag.menunameid = "inventory_menu";
                ViewBag.submenunameid = "stock_submenu";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //FIN HEADER


                var usuarios = dblim.Sys_Users.Where(c => c.ID_Company == 1 && c.Roles.Contains("Picker"));
                ViewBag.usuarios = usuarios;

                var project = dblim.Inv_Projects.Where(f => f.ID_project == idproj).FirstOrDefault();
                ViewBag.project = project;

                var history = dblim.Inv_Projects_Task.Where(c => c.ID_project == idproj);

                ViewBag.mensaje = msj;
                return View(history);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public ActionResult Binloc_general2(int idproj)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Inventory";
                ViewData["Page"] = "Projects";
                ViewBag.menunameid = "inventory_menu";
                ViewBag.submenunameid = "stock_submenu";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //FIN HEADER


                var usuarios = dblim.Sys_Users.Where(c => c.ID_Company == 1 && c.Roles.Contains("Picker"));
                ViewBag.usuarios = usuarios;

                var project = dblim.Inv_Projects.Where(f => f.ID_project == idproj).FirstOrDefault();
                ViewBag.project = project;

                var history = dblim.Inv_Projects_Task.Where(c => c.ID_project == idproj).Select(c => new Inv_ProjectsTasks_mod { ID_projects_task = c.ID_projects_task, ID_binloc = c.ID_binloc, Bin_location = c.Bin_location,
                    creation_date = c.creation_date, ID_userEnd = c.ID_userEnd, UserName = c.UserName, ID_status = c.ID_status, comments = c.comments, end_date = c.end_date, Count = c.Count, isselected = c.isselected, ItemCode = c.ItemCode, ItemName = c.ItemName,
                    UoM_code = c.UoM_code, UoM_entry = c.UoM_entry, Quantity = c.Quantity, UoM_code2 = c.UoM_code2, UoM_entry2 = c.UoM_entry2, Quantity2 = c.Quantity2,
                    UoM_code3 = c.UoM_code3,
                    UoM_entry3 = c.UoM_entry3,
                    Quantity3 = c.Quantity3,
                    UoM_code4 = c.UoM_code4,
                    UoM_entry4 = c.UoM_entry4,
                    Quantity4 = c.Quantity4, Final_quantity = c.Final_quantity, ID_empresa = c.ID_empresa, ID_project = c.ID_project, Area = c.Area, Internal_sort = c.Internal_sort, Type = c.Type, Aisle = c.Aisle, unitcost=0, stock=0, casescost=0, casesstock=0
                }).ToList();

                var products = history.Where(c => c.ItemCode != "").Select(c=>c.ItemCode).Distinct().ToArray();

                var cost = dlipro.BI_Dim_Products.Where(d => products.Contains(d.id)).ToArray();


                foreach (var item in history) {
                    if (item.ItemCode == "") {
                        item.ItemCode = "NOT SELECTED";
                    }
                    var existe = cost.Where(d => d.id == item.ItemCode).FirstOrDefault();
                    if (existe != null) {
                        item.unitcost = Convert.ToDecimal(existe.unitCost);
                        item.stock = Convert.ToDecimal(existe.Stock);
                    }
                }

                return View(history);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public class binlocationTasks
        {
            public int id { get; set; }
            public string BinCode { get; set; }
            public string WhsCode { get; set; }
            public string Aisle { get; set; }
            public string Position { get; set; }
            public string Level { get; set; }
            public string PalletPosition { get; set; }
            public string Type { get; set; }
            public string Area { get; set; }
            public int Order { get; set; }
            public int actualCount { get; set; }
            public List<TasksProj> LstTasks { get; set; }
        }
        public class TasksProj
        {
            public int id { get; set; }
            public int idbinloc { get; set; }
            public string BinCode { get; set; }
            public string Aisle { get; set; }
            public string Type { get; set; }
            public int ID_userEnd { get; set; }
            public string UserName { get; set; }
            public int ID_status { get; set; }
            public string comments { get; set; }
            public DateTime end_date { get; set; }
            public int Count { get; set; }
            public int Order { get; set; }
            public string Area { get; set; }
            public bool isselected { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
    
        }
        public ActionResult Binloc(int id,string messagetoken="")
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Inventory";
                ViewData["Page"] = "Projects";
                ViewBag.menunameid = "inventory_menu";
                ViewBag.submenunameid = "stock_submenu";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                var project = dblim.Inv_Projects.Where(f => f.ID_project == id).FirstOrDefault();
                if (project == null)
                {
                    return RedirectToAction("Projects", "Inventory", null);
                }
                ViewBag.project = project;
                //List<binlocationTasks> lstBinLocations = new List<binlocationTasks>();
                //lstBinLocations = dblim.Inv_Projects_BinLoc.Where(c => c.ID_project == id).Select(c => new binlocationTasks {
                //id=c.ID_binloc, BinCode=c.Bin_location, WhsCode=c.Warehouse, Aisle=c.Aisle, actualCount=c.Actual_counts, Position=c.Position, Level=c.Levelpst, PalletPosition=c.Pallet_position, Type=c.Type, Area =c.Area, Order=c.Internal_sort,
                //LstTasks= dblim.Inv_Projects_Task.Where(d=>d.ID_binloc==c.ID_binloc).Select(d=> new TasksProj { id=d.ID_projects_task, idbinloc=d.ID_binloc, BinCode=d.Bin_location, comments=d.comments, Count=d.Count,
                //end_date=d.end_date, ID_status=d.ID_status, ID_userEnd=d.ID_userEnd, isselected=d.isselected, ItemCode=d.ItemCode, ItemName=d.ItemName , UserName=d.UserName}).OrderBy(d=>d.Count).ToList()
                //}).OrderBy(d => d.Area).ThenBy(d => d.Order).ToList();
                List<TasksProj> lstBinLocations = new List<TasksProj>();
                lstBinLocations = dblim.Inv_Projects_Task.Where(d => d.ID_project == id && d.ID_userEnd==activeuser.ID_User).Select(d => new TasksProj
                {
                    id = d.ID_projects_task,
                    Area=d.Area,
                    Type=d.Type,
                    Aisle=d.Aisle,
                    idbinloc = d.ID_binloc,
                    BinCode = d.Bin_location,
                    comments = d.comments,
                    Count = d.Count,
                    Order = d.Internal_sort,
                    end_date=d.end_date, ID_status=d.ID_status, ID_userEnd=d.ID_userEnd, isselected=d.isselected, ItemCode=d.ItemCode, ItemName=d.ItemName , UserName=d.UserName}).OrderBy(d=>d.Type).ThenBy(d => d.Area).ThenBy(d=>d.Aisle).ThenBy(d => d.Order).ThenBy(d=>d.Count).ToList();


                    TempData["mensaje"] = messagetoken; //1-finalizado
                var totaltasks = dblim.Inv_Projects_Task.Where(c => c.ID_userEnd == activeuser.ID_User && c.ID_project == id).Count();
                var totaltasksFinished = dblim.Inv_Projects_Task.Where(c => c.ID_userEnd == activeuser.ID_User && c.ID_project == id && c.ID_status == 2).Count();
                double total = 0.0;
                if (totaltasks > 0)
                {
                    total = Convert.ToDouble(((decimal)totaltasksFinished / (decimal)totaltasks) * 100);
                }

                ViewBag.texttotaltasks = "(" + totaltasksFinished + "/" + totaltasks + ")";
                ViewBag.percenttotaltasks = total;
                //FIN HEADER
                return View(lstBinLocations);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }


        public class MyObj_SubCat

        {
            public int? id { get; set; }
            public string name { get; set; }
            public string category { get; set; }

        }
        public ActionResult Binloc_newlocation(int idproj)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Inventory";
                ViewData["Page"] = "Projects";
                ViewBag.menunameid = "inventory_menu";
                ViewBag.submenunameid = "stock_submenu";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //FIN HEADER

                var project = dblim.Inv_Projects.Where(f => f.ID_project == idproj).FirstOrDefault();
                ViewBag.project = project;


                var products = (from a in dlipro.BI_Dim_Products  select a);

                //var arraypro = products.Select(c => c.id).ToArray();

                //var generalProducts = (from a in dlipro.BI_Dim_Products where (a.Pepperi == "YES" && a.id_brand != 114 && a.Credits == "NO" && !arraypro.Contains(a.id)) select a).ToList();
                //ViewBag.generalProducts = generalProducts;

                var lstCategories = (from f in products select f.category_name).Distinct().OrderBy(c => c).ToList();
                ViewBag.lstCategories = lstCategories;

                var lstSubCategories = (from f in products select new MyObj_SubCat { id = f.id_subcategory, name = f.subcategory_name.Replace("'", ""), category = f.category_name }).Distinct().OrderBy(c => c.name).ToList();
                ViewBag.lstSubCategories = lstSubCategories;


                var lstBrands = (from f in products select new MyObj_SubCat { id = f.id_brand, name = f.Brand_Name.Replace("'", ""), category = f.subcategory_name.Replace("'", "") }).Distinct().OrderBy(c => c.name).ToList();
                ViewBag.lstBrands = lstBrands;

                var lstBinLocations = dlipro.OBIN.Where(a => !a.SL1Code.Contains("SYSTEM-BIN-LOCATION") && !a.SL1Code.Contains("ST")).Select(c => c.SL1Code).Distinct().ToList();
                ViewBag.binloclst = lstBinLocations;

                return View(products);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }



        public ActionResult Binloc_tasks(int idproj,int idtask=0, int idbin=0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Inventory";
                ViewData["Page"] = "Projects";
                ViewBag.menunameid = "inventory_menu";
                ViewBag.submenunameid = "stock_submenu";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //FIN HEADER
                //binlocationTasks whoisnext = new binlocationTasks();
                TasksProj whoisnext = new TasksProj();

                if (idtask == 0)
                {     

                    whoisnext = dblim.Inv_Projects_Task.Where(d => d.ID_project ==idproj && d.ID_userEnd == activeuser.ID_User && d.ID_status == 1).Select(d => new TasksProj
                    {
                        id = d.ID_projects_task,
                        Area = d.Area,
                        Type=d.Type,
                        Aisle=d.Aisle,
                        idbinloc = d.ID_binloc,
                        BinCode = d.Bin_location,
                        comments = d.comments,
                        Count = d.Count,
                        Order = d.Internal_sort,
                        end_date = d.end_date,
                        ID_status = d.ID_status,
                        ID_userEnd = d.ID_userEnd,
                        isselected = d.isselected,
                        ItemCode = d.ItemCode,
                        ItemName = d.ItemName,
                        UserName = d.UserName
                    }).OrderBy(d => d.Count).ThenBy(d => d.Type).ThenBy(d => d.Aisle).ThenBy(d => d.Order).FirstOrDefault();

                }
                else {

                    whoisnext = dblim.Inv_Projects_Task.Where(d => d.ID_project == idproj && d.ID_userEnd == activeuser.ID_User && d.ID_status == 1 && d.ID_projects_task == idtask).Select(d => new TasksProj
                    {
                        id = d.ID_projects_task,
                        Area = d.Area,
                        Type = d.Type,
                        Aisle = d.Aisle,
                        idbinloc = d.ID_binloc,
                        BinCode = d.Bin_location,
                        comments = d.comments,
                        Count = d.Count,
                        Order = d.Internal_sort,
                        end_date = d.end_date,
                        ID_status = d.ID_status,
                        ID_userEnd = d.ID_userEnd,
                        isselected = d.isselected,
                        ItemCode = d.ItemCode,
                        ItemName = d.ItemName,
                        UserName = d.UserName
                    }).OrderBy(d => d.Count).ThenBy(d => d.Type).ThenBy(d => d.Aisle).ThenBy(d => d.Order).FirstOrDefault();
                }
                if (whoisnext != null)
                {
                    var binlocation = dblim.Inv_Projects_BinLoc.Where(g => g.ID_project == idproj && g.ID_binloc == whoisnext.idbinloc).FirstOrDefault();
                    ViewBag.binloc = whoisnext;
                    if (binlocation.Type.Contains("Storage"))
                    {
                        ViewBag.isstorage = "1";
                    }
                    else { ViewBag.isstorage = "0"; }
                    
                }
                else {
                    //No hay mas tareas
                    return RedirectToAction("Binloc", "Inventory", new { id = idproj, messagetoken= "1" });
                }


                var project = dblim.Inv_Projects.Where(f => f.ID_project == idproj).FirstOrDefault();
                ViewBag.project = project;

                
                var products = (from a in dlipro.BI_Dim_Products where (a.StorageType==whoisnext.Area) select a);

                var arraypro = products.Select(c => c.id).ToArray();

                var generalProducts = (from a in dlipro.BI_Dim_Products where (!arraypro.Contains(a.id)) select a).ToList();
                ViewBag.generalProducts = generalProducts;

                var lstCategories = (from f in products select f.category_name).Distinct().OrderBy(c => c).ToList();
                ViewBag.lstCategories = lstCategories;

                var lstSubCategories = (from f in products select new MyObj_SubCat { id = f.id_subcategory, name = f.subcategory_name.Replace("'", ""), category = f.category_name }).Distinct().OrderBy(c => c.name).ToList();
                ViewBag.lstSubCategories = lstSubCategories;


                var lstBrands = (from f in products select new MyObj_SubCat { id = f.id_brand, name = f.Brand_Name.Replace("'", ""), category = f.subcategory_name.Replace("'", "") }).Distinct().OrderBy(c => c.name).ToList();
                ViewBag.lstBrands = lstBrands;

                return View(products);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }

        public ActionResult Binloc_tasksEdit(int idproj, int idtask = 0, int idbin = 0)
        {
            if (generalClass.checkSession())
            {
                Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                //HEADER
                //ACTIVE PAGES
                ViewData["Menu"] = "Inventory";
                ViewData["Page"] = "Projects";
                ViewBag.menunameid = "inventory_menu";
                ViewBag.submenunameid = "stock_submenu";
                List<string> s = new List<string>(activeuser.Departments.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstDepartments = JsonConvert.SerializeObject(s);
                List<string> r = new List<string>(activeuser.Roles.Split(new string[] { "," }, StringSplitOptions.None));
                ViewBag.lstRoles = JsonConvert.SerializeObject(r);
                //NOTIFICATIONS
                DateTime now = DateTime.Today;
                List<Tb_Alerts> lstAlerts = (from a in dblim.Tb_Alerts where (a.ID_user == activeuser.ID_User && a.Active == true && a.Date == now) select a).OrderByDescending(x => x.Date).Take(5).ToList();
                ViewBag.lstAlerts = lstAlerts;

                ViewData["nameUser"] = activeuser.Name + " " + activeuser.Lastname;
                //FIN HEADER
                //binlocationTasks whoisnext = new binlocationTasks();
                TasksProj whoisnext = new TasksProj();

                if (idtask == 0)
                {

                    whoisnext = dblim.Inv_Projects_Task.Where(d => d.ID_project == idproj && d.ID_userEnd == activeuser.ID_User).Select(d => new TasksProj
                    {
                        id = d.ID_projects_task,
                        Area = d.Area,
                        Type = d.Type,
                        Aisle = d.Aisle,
                        idbinloc = d.ID_binloc,
                        BinCode = d.Bin_location,
                        comments = d.comments,
                        Count = d.Count,
                        Order = d.Internal_sort,
                        end_date = d.end_date,
                        ID_status = d.ID_status,
                        ID_userEnd = d.ID_userEnd,
                        isselected = d.isselected,
                        ItemCode = d.ItemCode,
                        ItemName = d.ItemName,
                        UserName = d.UserName
                    }).OrderBy(d => d.Count).ThenBy(d => d.Type).ThenBy(d => d.Aisle).ThenBy(d => d.Order).FirstOrDefault();

                }
                else
                {

                    whoisnext = dblim.Inv_Projects_Task.Where(d => d.ID_project == idproj && d.ID_userEnd == activeuser.ID_User && d.ID_projects_task == idtask).Select(d => new TasksProj
                    {
                        id = d.ID_projects_task,
                        Area = d.Area,
                        Type = d.Type,
                        Aisle = d.Aisle,
                        idbinloc = d.ID_binloc,
                        BinCode = d.Bin_location,
                        comments = d.comments,
                        Count = d.Count,
                        Order = d.Internal_sort,
                        end_date = d.end_date,
                        ID_status = d.ID_status,
                        ID_userEnd = d.ID_userEnd,
                        isselected = d.isselected,
                        ItemCode = d.ItemCode,
                        ItemName = d.ItemName,
                        UserName = d.UserName
                    }).OrderBy(d => d.Count).ThenBy(d => d.Type).ThenBy(d => d.Aisle).ThenBy(d => d.Order).FirstOrDefault();
                }
                if (whoisnext != null)
                {
                    var binlocation = dblim.Inv_Projects_BinLoc.Where(g => g.ID_project == idproj && g.ID_binloc == whoisnext.idbinloc).FirstOrDefault();
                    ViewBag.binloc = whoisnext;
                    if (binlocation.Type.Contains("Storage"))
                    {
                        ViewBag.isstorage = "1";
                    }
                    else { ViewBag.isstorage = "0"; }

                }
                else
                {
                    //No hay mas tareas
                    return RedirectToAction("Binloc", "Inventory", new { id = idproj, messagetoken = "1" });
                }


                var project = dblim.Inv_Projects.Where(f => f.ID_project == idproj).FirstOrDefault();
                ViewBag.project = project;


                var products = (from a in dlipro.BI_Dim_Products where (a.StorageType == whoisnext.Area) select a);

                var arraypro = products.Select(c => c.id).ToArray();

                var generalProducts = (from a in dlipro.BI_Dim_Products where (!arraypro.Contains(a.id)) select a).ToList();
                ViewBag.generalProducts = generalProducts;

                var lstCategories = (from f in products select f.category_name).Distinct().OrderBy(c => c).ToList();
                ViewBag.lstCategories = lstCategories;

                var lstSubCategories = (from f in products select new MyObj_SubCat { id = f.id_subcategory, name = f.subcategory_name.Replace("'", ""), category = f.category_name }).Distinct().OrderBy(c => c.name).ToList();
                ViewBag.lstSubCategories = lstSubCategories;


                var lstBrands = (from f in products select new MyObj_SubCat { id = f.id_brand, name = f.Brand_Name.Replace("'", ""), category = f.subcategory_name.Replace("'", "") }).Distinct().OrderBy(c => c.name).ToList();
                ViewBag.lstBrands = lstBrands;

                return View(products);

            }
            else
            {

                return RedirectToAction("Login", "Home", new { access = false });

            }
        }
        public ActionResult SaveandExit_task(string idtask, string ItemCode,string ItemName, string seluom1,int seluom1Code, int seluom1Quantity, string seluom2, int seluom2Code, int seluom2Quantity, string seluom3, int seluom3Code, int seluom3Quantity, string seluom4, int seluom4Code, int seluom4Quantity, string comment)
        {
            try
            {
                if (idtask != "")
                {
                    var taskid = Convert.ToInt32(idtask);
                    var bintask = (from a in dblim.Inv_Projects_Task where (a.ID_projects_task == taskid) select a).FirstOrDefault();

                    bintask.ID_status = 2;
                    bintask.comments = comment;
                    bintask.ItemCode = ItemCode;
                    bintask.ItemName = ItemName;
                    bintask.end_date = DateTime.UtcNow;
                    bintask.UoM_code = seluom1;
                    bintask.UoM_entry = seluom1Code;
                    bintask.Quantity = seluom1Quantity;
                    bintask.UoM_code2 = seluom2;
                    bintask.UoM_entry2 = seluom2Code;
                    bintask.Quantity2 = seluom2Quantity;
                    bintask.UoM_code3 = seluom3;
                    bintask.UoM_entry3 = seluom3Code;
                    bintask.Quantity3 = seluom3Quantity;
                    bintask.UoM_code4 = seluom4;
                    bintask.UoM_entry4 = seluom4Code;
                    bintask.Quantity4 = seluom4Quantity;
                    
                    var finalq = 0;
                    if (ItemCode != "0") {
                        //calcular final quantity
                        if (seluom1.Contains("CASE"))
                        {
                            var selectuomconv = dlipro.OpenSalesOrders_DetailsUOM.Where(c => c.ItemCode == ItemCode && c.UomEntry == seluom1Code).FirstOrDefault();
                            if (selectuomconv == null) { finalq = finalq + seluom1Quantity; }
                            else
                            {
                                finalq = finalq + Convert.ToInt32(seluom1Quantity * selectuomconv.Units);
                            }
                        }
                        else
                        {
                            finalq = finalq + seluom1Quantity;
                        }
                        if (seluom2.Contains("CASE"))
                        {
                            var selectuomconv2 = dlipro.OpenSalesOrders_DetailsUOM.Where(c => c.ItemCode == ItemCode && c.UomEntry == seluom2Code).FirstOrDefault();
                            if (selectuomconv2 == null) { finalq = finalq + seluom2Quantity; }
                            else
                            {
                                finalq = finalq + Convert.ToInt32(seluom2Quantity * selectuomconv2.Units);
                            }
                        }
                        else
                        {
                            finalq = finalq + seluom2Quantity;
                        }
                        if (seluom3.Contains("CASE"))
                        {
                            var selectuomconv3 = dlipro.OpenSalesOrders_DetailsUOM.Where(c => c.ItemCode == ItemCode && c.UomEntry == seluom3Code).FirstOrDefault();
                            if (selectuomconv3 == null) { finalq = finalq + seluom3Quantity; }
                            else
                            {
                                finalq = finalq + Convert.ToInt32(seluom3Quantity * selectuomconv3.Units);
                            }
                        }
                        else
                        {
                            finalq = finalq + seluom3Quantity;
                        }
                        if (seluom4.Contains("CASE"))
                        {
                            var selectuomconv4 = dlipro.OpenSalesOrders_DetailsUOM.Where(c => c.ItemCode == ItemCode && c.UomEntry == seluom4Code).FirstOrDefault();
                            if (selectuomconv4 == null) { finalq = finalq + seluom4Quantity; }
                            else
                            {
                                finalq = finalq + Convert.ToInt32(seluom4Quantity * selectuomconv4.Units);
                            }
                        }
                        else
                        {
                            finalq = finalq + seluom4Quantity;
                        }
                    }


                    bintask.Final_quantity = finalq;
                    //
                    dblim.Entry(bintask).State = EntityState.Modified;
                    dblim.SaveChanges();


                    var binlocation = (from a in dblim.Inv_Projects_BinLoc where (a.ID_binloc == bintask.ID_binloc) select a).FirstOrDefault();
                    //actualizamos el count de ser necesario
                    if (bintask.Count == binlocation.Count)
                    {

                    }
                    else {
                        if (bintask.Count < binlocation.Count) {
                            binlocation.Actual_counts = binlocation.Actual_counts+ 1;

                            if (binlocation.Actual_counts > binlocation.Count)
                            {
                                //binlocation.ID_status = 1;
                            }

                            dblim.Entry(binlocation).State = EntityState.Modified;
                            dblim.SaveChanges();
                        }
                    }

                    string result = "SUCCESS";
                    return Json(result, JsonRequestBehavior.AllowGet);

                }
                else {
                    return Json("error", JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult SaveandExit_newlocation(string idproj,int conteo,string aisle, string ItemCode, string ItemName, string seluom1, int seluom1Code, int seluom1Quantity, string seluom2, int seluom2Code, int seluom2Quantity, string seluom3, int seluom3Code, int seluom3Quantity, string seluom4, int seluom4Code, int seluom4Quantity, string comment)
        {
            try
            {
                if (generalClass.checkSession())
                {
                    Sys_Users activeuser = Session["activeUser"] as Sys_Users;
                    if (idproj != "")
                    {
                        var projid = Convert.ToInt32(idproj);
                        var projss = (from a in dblim.Inv_Projects where (a.ID_project == projid) select a).FirstOrDefault();

                        Inv_Projects_BinLoc items = new Inv_Projects_BinLoc();
                        Inv_Projects_Task bintask = new Inv_Projects_Task();


                        items.Bin_IDSAP = 0;
                        items.Count = 0;//max conteo
                        items.Bin_location = "01" + aisle + "GENERICO";
                        items.Warehouse = "01";
                        items.Aisle = aisle;
                        items.Position = "NA";
                        items.Levelpst = "NA";
                        items.Pallet_position = "NA";
                        items.Area = "NA"; 
                        items.Type = "NA"; 
                        items.Internal_sort = 9999; 
                        items.creation_date = DateTime.UtcNow;
                        items.ID_userCreate = activeuser.ID_User;
                        items.UserNameCreate = activeuser.Name.ToUpper() + " " + activeuser.Lastname.ToUpper();
                        items.end_date = DateTime.UtcNow;
                        items.Actual_counts = conteo; //conteo seleccionado por usuario
                        items.ID_status = 0;
                        items.isfinished = false;
                        items.comments = "";
                        items.ID_project = projss.ID_project;
                        items.ID_empresa = activeuser.ID_Company;
                        items.Error = false;
                        items.MensajeError = "";
                        items.OnSAP = false;
                        items.ID_userAssigned= activeuser.ID_User;
                        items.UserNameAssigned= activeuser.Name.ToUpper() + " " + activeuser.Lastname.ToUpper();
                        dblim.Inv_Projects_BinLoc.Add(items);
                        dblim.SaveChanges();


 
                            Inv_Projects_Task newtask = new Inv_Projects_Task();
                            newtask.ID_binloc = items.ID_binloc;
                            newtask.Bin_location = items.Bin_location;
                            newtask.creation_date = DateTime.UtcNow;
                        newtask.ID_userEnd = activeuser.ID_User;
                        newtask.UserName= activeuser.Name.ToUpper() + " " + activeuser.Lastname.ToUpper();
                        newtask.Count = conteo;
                        newtask.ID_status = 2;
                        newtask.isselected = false;
                            newtask.ID_empresa = activeuser.ID_Company;
                            newtask.ID_project = items.ID_project;
                            newtask.Internal_sort = items.Internal_sort;
                            newtask.Type = items.Type;
                            newtask.Aisle = items.Aisle;
                            newtask.Area = items.Area;
                        newtask.Internal_sort = 9999;
                        newtask.comments = comment;
                        newtask.ItemCode = ItemCode;
                        newtask.ItemName = ItemName;
                        newtask.end_date = DateTime.UtcNow;
                        newtask.UoM_code = seluom1;
                        newtask.UoM_entry = seluom1Code;
                        newtask.Quantity = seluom1Quantity;
                        newtask.UoM_code2 = seluom2;
                        newtask.UoM_entry2 = seluom2Code;
                        newtask.Quantity2 = seluom2Quantity;
                        newtask.UoM_code3 = seluom3;
                        newtask.UoM_entry3 = seluom3Code;
                        newtask.Quantity3 = seluom3Quantity;
                        newtask.UoM_code4 = seluom4;
                        newtask.UoM_entry4 = seluom4Code;
                        newtask.Quantity4 = seluom4Quantity;
                       
                        var finalq = 0;
                        if (ItemCode != "0")
                        {
                            //calcular final quantity
                            if (seluom1.Contains("CASE"))
                            {
                                var selectuomconv = dlipro.OpenSalesOrders_DetailsUOM.Where(c => c.ItemCode == ItemCode && c.UomEntry == seluom1Code).FirstOrDefault();
                                if (selectuomconv == null) { finalq = finalq + seluom1Quantity; }
                                else
                                {
                                    finalq = finalq + Convert.ToInt32(seluom1Quantity * selectuomconv.Units);
                                }
                            }
                            else
                            {
                                finalq = finalq + seluom1Quantity;
                            }
                            if (seluom2.Contains("CASE"))
                            {
                                var selectuomconv2 = dlipro.OpenSalesOrders_DetailsUOM.Where(c => c.ItemCode == ItemCode && c.UomEntry == seluom2Code).FirstOrDefault();
                                if (selectuomconv2 == null) { finalq = finalq + seluom2Quantity; }
                                else
                                {
                                    finalq = finalq + Convert.ToInt32(seluom2Quantity * selectuomconv2.Units);
                                }
                            }
                            else
                            {
                                finalq = finalq + seluom2Quantity;
                            }
                            if (seluom3.Contains("CASE"))
                            {
                                var selectuomconv3 = dlipro.OpenSalesOrders_DetailsUOM.Where(c => c.ItemCode == ItemCode && c.UomEntry == seluom3Code).FirstOrDefault();
                                if (selectuomconv3 == null) { finalq = finalq + seluom3Quantity; }
                                else
                                {
                                    finalq = finalq + Convert.ToInt32(seluom3Quantity * selectuomconv3.Units);
                                }
                            }
                            else
                            {
                                finalq = finalq + seluom3Quantity;
                            }
                            if (seluom4.Contains("CASE"))
                            {
                                var selectuomconv4 = dlipro.OpenSalesOrders_DetailsUOM.Where(c => c.ItemCode == ItemCode && c.UomEntry == seluom4Code).FirstOrDefault();
                                if (selectuomconv4 == null) { finalq = finalq + seluom4Quantity; }
                                else
                                {
                                    finalq = finalq + Convert.ToInt32(seluom4Quantity * selectuomconv4.Units);
                                }
                            }
                            else
                            {
                                finalq = finalq + seluom4Quantity;
                            }
                        }


                        newtask.Final_quantity = finalq;
                        //

                        dblim.Inv_Projects_Task.Add(newtask);
                        dblim.SaveChanges();


                        string result = "SUCCESS";
                        return Json(result, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        return Json("error", JsonRequestBehavior.AllowGet);
                    }
                }
                else {
                    return Json("error", JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult SaveandContinue_task(string idtask, string ItemCode, string ItemName, string seluom1, int seluom1Code, int seluom1Quantity, string seluom2, int seluom2Code, int seluom2Quantity, string seluom3, int seluom3Code, int seluom3Quantity, string seluom4, int seluom4Code, int seluom4Quantity, string comment)
        {
            try
            {
                if (idtask != "")
                {
                    if (generalClass.checkSession())
                    {
                        Sys_Users activeuser = Session["activeUser"] as Sys_Users;




                        var taskid = Convert.ToInt32(idtask);
                        var bintask = (from a in dblim.Inv_Projects_Task where (a.ID_projects_task == taskid) select a).FirstOrDefault();



                        //buscamos el siguiente
                        var whoisnext = 
                             dblim.Inv_Projects_Task.Where(d => d.ID_project==bintask.ID_project && d.ID_status == 1 && d.ID_userEnd==activeuser.ID_User).OrderBy(d => d.Count).ThenBy(d => d.Type).ThenBy(d => d.Aisle).ThenBy(d => d.Internal_sort).ToList();

                        var actualid = bintask.ID_projects_task;
                        var next = whoisnext.SkipWhile(obj => obj.ID_projects_task != actualid).Skip(1).FirstOrDefault();
                        //****


                        bintask.ID_status = 2;
                        bintask.comments = comment;
                        bintask.ItemCode = ItemCode;
                        bintask.ItemName = ItemName;
                        bintask.end_date = DateTime.UtcNow;
                        bintask.UoM_code = seluom1;
                        bintask.UoM_entry = seluom1Code;
                        bintask.Quantity = seluom1Quantity;
                        bintask.UoM_code2 = seluom2;
                        bintask.UoM_entry2 = seluom2Code;
                        bintask.Quantity2 = seluom2Quantity;
                        bintask.UoM_code3 = seluom3;
                        bintask.UoM_entry3 = seluom3Code;
                        bintask.Quantity3 = seluom3Quantity;
                        bintask.UoM_code4 = seluom4;
                        bintask.UoM_entry4 = seluom4Code;
                        bintask.Quantity4 = seluom4Quantity;

                        var finalq = 0;

                        //calcular final quantity
                        if (seluom1.Contains("CASE"))
                        {
                            var selectuomconv = dlipro.OpenSalesOrders_DetailsUOM.Where(c => c.ItemCode == ItemCode && c.UomEntry==seluom1Code).FirstOrDefault();
                            if (selectuomconv == null) { finalq = finalq + seluom1Quantity; } else {
                                finalq = finalq + Convert.ToInt32(seluom1Quantity * selectuomconv.Units);
                            }
                        }
                        else {
                            finalq = finalq + seluom1Quantity;
                        }
                        if (seluom2.Contains("CASE"))
                        {
                            var selectuomconv2 = dlipro.OpenSalesOrders_DetailsUOM.Where(c => c.ItemCode == ItemCode && c.UomEntry == seluom2Code).FirstOrDefault();
                            if (selectuomconv2 == null) { finalq = finalq + seluom2Quantity; }
                            else
                            {
                                finalq = finalq + Convert.ToInt32(seluom2Quantity * selectuomconv2.Units);
                            }
                        }
                        else
                        {
                            finalq = finalq + seluom2Quantity;
                        }
                        if (seluom3.Contains("CASE"))
                        {
                            var selectuomconv3 = dlipro.OpenSalesOrders_DetailsUOM.Where(c => c.ItemCode == ItemCode && c.UomEntry == seluom3Code).FirstOrDefault();
                            if (selectuomconv3 == null) { finalq = finalq + seluom3Quantity; }
                            else
                            {
                                finalq = finalq + Convert.ToInt32(seluom3Quantity * selectuomconv3.Units);
                            }
                        }
                        else
                        {
                            finalq = finalq + seluom3Quantity;
                        }
                        if (seluom4.Contains("CASE"))
                        {
                            var selectuomconv4 = dlipro.OpenSalesOrders_DetailsUOM.Where(c => c.ItemCode == ItemCode && c.UomEntry == seluom4Code).FirstOrDefault();
                            if (selectuomconv4 == null) { finalq = finalq + seluom4Quantity; }
                            else
                            {
                                finalq = finalq + Convert.ToInt32(seluom4Quantity * selectuomconv4.Units);
                            }
                        }
                        else
                        {
                            finalq = finalq + seluom4Quantity;
                        }

                        bintask.Final_quantity = finalq;
                        //
                        
                        dblim.Entry(bintask).State = EntityState.Modified;
                        dblim.SaveChanges();


                        var binlocation = (from a in dblim.Inv_Projects_BinLoc where (a.ID_binloc == bintask.ID_binloc) select a).FirstOrDefault();
                        //actualizamos el count de ser necesario
                        if (bintask.Count == binlocation.Count)
                        {

                        }
                        else
                        {
                            if (bintask.Count < binlocation.Count)
                            {
                                binlocation.Actual_counts = binlocation.Actual_counts + 1;

                                if (binlocation.Actual_counts > binlocation.Count)
                                {
                                    //binlocation.ID_status = 1;
                                }
                               


                                dblim.Entry(binlocation).State = EntityState.Modified;
                                dblim.SaveChanges();
                            }
                        }


                        if (next == null)
                        {
                            var result = new { bin = 0, task = 0, result= "SUCCESS NO REDIRECT" };
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                        else {

                                
                            var result = new { bin = next.ID_binloc, task = next.ID_projects_task, result = "SUCCESS" };
                            return Json(result, JsonRequestBehavior.AllowGet);

                 
                        }

                    
                    }
                    else {
                        var result = new { bin = 0, task = 0, result = "Error" };
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var result = new { bin = 0, task = 0, result = "Error" };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                var result = new { bin = 0, task = 0, result = "Error" };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
    }
}