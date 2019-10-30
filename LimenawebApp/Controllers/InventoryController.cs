using LimenawebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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


                var isadmin = isAdminRole(activeuser.Roles);
                List<Inv_Projects> lstprojects = new List<Inv_Projects>();
                if (isadmin)
                {
                    lstprojects = dblim.Inv_Projects.ToList();
                }
                else {
                    var idu = activeuser.ID_User.ToString();
                    lstprojects = dblim.Inv_Projects.Where(c => c.IDusers_assigned.Contains(idu)).ToList();
                }

                ViewBag.adminRole = isadmin;
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




        public ActionResult Binloc_master()
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
                return View();

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
            public int ID_userEnd { get; set; }
            public string UserName { get; set; }
            public int ID_status { get; set; }
            public string comments { get; set; }
            public DateTime end_date { get; set; }
            public int Count { get; set; }
            public bool isselected { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
    
        }
        public ActionResult Binloc(int id)
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
                List<binlocationTasks> lstBinLocations = new List<binlocationTasks>();
                lstBinLocations = dblim.Inv_Projects_BinLoc.Where(c => c.ID_project == id).Select(c => new binlocationTasks {
                id=c.ID_binloc, BinCode=c.Bin_location, WhsCode=c.Warehouse, Aisle=c.Aisle, actualCount=c.Actual_counts, Position=c.Position, Level=c.Levelpst, PalletPosition=c.Pallet_position, Type=c.Type, Area =c.Area, Order=c.Internal_sort,
                LstTasks= dblim.Inv_Projects_Task.Where(d=>d.ID_binloc==c.ID_binloc).Select(d=> new TasksProj { id=d.ID_projects_task, idbinloc=d.ID_binloc, BinCode=d.Bin_location, comments=d.comments, Count=d.Count,
                end_date=d.end_date, ID_status=d.ID_status, ID_userEnd=d.ID_userEnd, isselected=d.isselected, ItemCode=d.ItemCode, ItemName=d.ItemName , UserName=d.UserName}).ToList()
                }).OrderBy(d=>d.Order).ToList();


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
                binlocationTasks whoisnext = new binlocationTasks();


                if (idtask == 0)
                {
                    whoisnext = dblim.Inv_Projects_BinLoc.Where(c => c.ID_project == idproj && c.ID_status == 0).Select(c => new binlocationTasks
                    {
                        id = c.ID_binloc,
                        BinCode = c.Bin_location,
                        WhsCode = c.Warehouse,
                        Aisle = c.Aisle,
                        Position = c.Position,
                        Level = c.Levelpst,
                        PalletPosition = c.Pallet_position,
                        actualCount = c.Actual_counts,
                        Type = c.Type,
                        Area = c.Area,
                        Order = c.Internal_sort,
                        LstTasks = dblim.Inv_Projects_Task.Where(d => d.ID_binloc == c.ID_binloc && d.ID_status==1).Select(d => new TasksProj
                        {
                            id = d.ID_projects_task,
                            idbinloc = d.ID_binloc,
                            BinCode = d.Bin_location,
                            comments = d.comments,
                            Count = d.Count,
                            end_date = d.end_date,
                            ID_status = d.ID_status,
                            ID_userEnd = d.ID_userEnd,
                            isselected = d.isselected,
                            ItemCode = d.ItemCode,
                            ItemName = d.ItemName,
                            UserName = d.UserName
                        }).OrderBy(d=>d.Count).ToList()
                    }).OrderBy(d => d.actualCount).ThenBy(d=>d.Order).FirstOrDefault();

                    if (whoisnext != null) {
                        ViewBag.binloc = whoisnext;
                    }
              
                }
                else {
                    whoisnext = dblim.Inv_Projects_BinLoc.Where(c => c.ID_project == idproj && c.ID_status == 0 && c.ID_binloc==idbin).Select(c => new binlocationTasks
                    {
                        id = c.ID_binloc,
                        BinCode = c.Bin_location,
                        WhsCode = c.Warehouse,
                        Aisle = c.Aisle,
                        Position = c.Position,
                        Level = c.Levelpst,
                        PalletPosition = c.Pallet_position,
                        actualCount = c.Actual_counts,
                        Type = c.Type,
                        Area = c.Area,
                        Order = c.Internal_sort,
                        LstTasks = dblim.Inv_Projects_Task.Where(d => d.ID_binloc == c.ID_binloc && d.ID_status == 1 && d.ID_projects_task==idtask).Select(d => new TasksProj
                        {
                            id = d.ID_projects_task,
                            idbinloc = d.ID_binloc,
                            BinCode = d.Bin_location,
                            comments = d.comments,
                            Count = d.Count,
                            end_date = d.end_date,
                            ID_status = d.ID_status,
                            ID_userEnd = d.ID_userEnd,
                            isselected = d.isselected,
                            ItemCode = d.ItemCode,
                            ItemName = d.ItemName,
                            UserName = d.UserName
                        }).OrderBy(d => d.Count).ToList()
                    }).OrderBy(d => d.actualCount).ThenBy(d => d.Order).FirstOrDefault();

                    if (whoisnext != null)
                    {
                        ViewBag.binloc = whoisnext;
                    }
                }
                var project = dblim.Inv_Projects.Where(f => f.ID_project == idproj).FirstOrDefault();
                ViewBag.project = project;


                var products = (from a in dlipro.BI_Dim_Products where (a.Pepperi == "YES" && a.id_brand != 114 && a.Credits == "NO") select a);

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
    }
}