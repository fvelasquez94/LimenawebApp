(function ($) {
	"use strict";
    var warehouse = warehousesel;
    var flagbtn = 0;
  var calendar,
  e,
  noticed = false,
  option = {
    header: {
      left: 'title, prev, next',
      center: '',
      right: ''
      },
      //minTime: "04:00:00",
      //maxTime: "22:00:00",
    contentHeight: 'auto',
      defaultView: 'basicWeek',

      validRange: {
          start: visibleInit,
          end: visibleEnd
      },
    defaultDate: moment().format('YYYY-MM-DD'),
    editable: false,
    eventLimit: false,
    viewRender: function (view, element) {
      // style

    },
    eventRender: function(event, element) {
      // render
        $(element).css("margin-top", "5px");


        $(element).css("margin-bottom", "5px");
        element.find('.fc-content').append('<div class="mt-1 text-muted">' + "description" + '</div>');

        if (event.isfinished == "Y") {

            if (warehouse == "01") {
                if (event.Warehouse == "02") {
                    element.find('.fc-content').html("<i class='fa fa-truck text-dark'></i><i class='fa fa-check-circle text-success'></i><b>" + "ROUTE TO LOUISVILLE, KY" + "</b>");
                } else {
                    element.find('.fc-content').html("<i class='fa fa-check-circle text-success'></i><b>" + event.title + "</b>");
                }
            } else {
                element.find('.fc-content').html("<i class='fa fa-check-circle text-success'></i><b>" + event.title + "</b>");
            }


        }
        else {


            if (warehouse == "01") {
                if (event.Warehouse == "02") {
                    element.find('.fc-content').html("<i class='fa fa-truck text-dark'></i><b>" + "ROUTE TO LOUISVILLE, KY" + "</b>");
                } else {
                    element.find('.fc-content').html("<b>" + event.title + "</b>");
                }
            } else {
                element.find('.fc-content').html("<b>" + event.title + "</b>");
            }


        }


        if (warehouse == "01") {
            if (event.Warehouse == "02") {

                $(element).css("backgroundColor", "#fdfef6");
                element.find('.fc-content').append("<br/>Driver:");
                if (event.driver_WHS == "") {
                    element.find('.fc-content').append("<br/>" + "NOT ASSIGNED");
                } else {
                    element.find('.fc-content').append("<br/>" + event.driver_WHS);
                }


                element.find('.fc-content').append("<br/>Truck:");
                if (event.truck_WHS == "") {
                    element.find('.fc-content').append("<br/>" + "NOT ASSIGNED");
                } else {
                    element.find('.fc-content').append("<br/>" + event.truck_WHS);
                }



            } else {
                element.find('.fc-content').append("<br/>Driver:");
                element.find('.fc-content').append("" + event.driver);

                element.find('.fc-content').append("<br/>Truck:");
                element.find('.fc-content').append("" + event.truck);
                element.find('.fc-content').append("<br/>Route Leader:");
                element.find('.fc-content').append("" + event.route_leader);
                element.find('.fc-content').append("<br/>Departure:");
                element.find('.fc-content').append("" + new Date(parseInt(event.departure.substr(6))).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
                element.find('.fc-content').append("<br/>Total Amount: ");
                element.find('.fc-content').append(formatter.format(event.amount));
                element.find('.fc-content').append("<br/>Total Orders: ");
                element.find('.fc-content').append("" + event.orderscount);
                element.find('.fc-content').append("<br/>Total Customers: ");
                element.find('.fc-content').append("" + event.customerscount);
            }
        } else {
            element.find('.fc-content').append("<br/>Driver:");
            element.find('.fc-content').append("" + event.driver);

            element.find('.fc-content').append("<br/>Truck:");
            element.find('.fc-content').append("" + event.truck);
            element.find('.fc-content').append("<br/>Route Leader:");
            element.find('.fc-content').append("" + event.route_leader);
            element.find('.fc-content').append("<br/>Departure:");
            element.find('.fc-content').append("" + new Date(parseInt(event.departure.substr(6))).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
            element.find('.fc-content').append("<br/>Total Amount: ");
            element.find('.fc-content').append(formatter.format(event.amount));
            element.find('.fc-content').append("<br/>Total Orders: ");
            element.find('.fc-content').append("" + event.orderscount);
            element.find('.fc-content').append("<br/>Total Customers: ");
            element.find('.fc-content').append("" + event.customerscount);

        }

        if (warehouse == "01") {
            if (event.Warehouse == "02") {

                /*For List view*/

                if (event.isfinished == "Y") { element.find('.fc-list-item-title').html("<i class='fa fa-truck text-dark'></i><i class='fa fa-check-circle text-success'></i><b>" + "ROUTE TO LOUISVILLE, KY" + "</b>"); } else { element.find('.fc-list-item-title').html("<i class='fa fa-truck text-dark'></i><b>" + "ROUTE TO LOUISVILLE, KY" + "</b>"); }

                element.find('.fc-list-item-title').append("<br/>Driver:");
                element.find('.fc-list-item-title').append("<br/>" + event.driver_WHS);
                element.find('.fc-list-item-title').append("<br/>Truck:");
                element.find('.fc-list-item-title').append("<br/>" + event.truck_WHS);




            } else {
                /*For List view*/

                if (event.isfinished == "Y") { element.find('.fc-list-item-title').html("<i class='fa fa-check-circle text-success'></i><b>" + event.title + "</b>"); } else { element.find('.fc-list-item-title').html("<b>" + event.title + "</b>"); }

                element.find('.fc-list-item-title').append("<br/>Driver:");
                element.find('.fc-list-item-title').append("<br/>" + event.driver);
                element.find('.fc-list-item-title').append("<br/>Truck:");
                element.find('.fc-list-item-title').append("<br/>" + event.truck);
                element.find('.fc-list-item-title').append("<br/>Route Leader:");
                element.find('.fc-list-item-title').append("" + event.route_leader);
                element.find('.fc-list-item-title').append("<br/>Departure:");
                element.find('.fc-list-item-title').append("<br/>" + event.departure);
                element.find('.fc-list-item-title').append("<br/>Total Amount: ");
                element.find('.fc-list-item-title').append(formatter.format(event.amount));
                element.find('.fc-list-item-title').append("<br/>Total Orders: ");
                element.find('.fc-list-item-title').append("" + event.orderscount);
                element.find('.fc-list-item-title').append("<br/>Total Customers: ");
                element.find('.fc-list-item-title').append("" + event.customerscount);
            }
        } else {
            /*For List view*/

            if (event.isfinished == "Y") { element.find('.fc-list-item-title').html("<i class='fa fa-check-circle text-success'></i><b>" + event.title + "</b>"); } else { element.find('.fc-list-item-title').html("<b>" + event.title + "</b>"); }

            element.find('.fc-list-item-title').append("<br/>Driver:");

            element.find('.fc-list-item-title').append("<br/>" + event.driver);
            element.find('.fc-list-item-title').append("<br/>Route Leader:");
            element.find('.fc-list-item-title').append("<br/>" + event.route_leader);
            element.find('.fc-list-item-title').append("<br/>Truck:");
            element.find('.fc-list-item-title').append("<br/>" + event.truck);
            element.find('.fc-list-item-title').append("<br/>Departure:");
            element.find('.fc-list-item-title').append("<br/>" + event.departure);
            element.find('.fc-list-item-title').append("<br/>Total Amount: ");
            element.find('.fc-list-item-title').append(formatter.format(event.amount));
            element.find('.fc-list-item-title').append("<br/>Total Orders: ");
            element.find('.fc-list-item-title').append("" + event.orderscount);
            element.find('.fc-list-item-title').append("<br/>Total Customers: ");
            element.find('.fc-list-item-title').append("" + event.customerscount);
            element.find('.fc-list-item-title').append("<br/>Total Extra: ");
            element.find('.fc-list-item-title').append("" + formatter.format(event.extra));
            element.find('.fc-list-item-title').append("<br/>Total Cases: ");
            element.find('.fc-list-item-title').append("" + formatterNoCurrency.format(event.AVGEach));
        }




        //element.find('.fc-content').append('<div class="d-flex my-3 avatar-group">' + '<a href="#" title="' + event.driver + '" class="w-24"><img  style="width:30px !important" src="../Content/assets/images/marker/delivery-man.png"></a>' + '<a style="margin-left:15px;" href="#" title="' + event.truck + '" class="w-24"><img  style="width:30px !important" src="../Content/assets/images/marker/truck.png"></a>' +'</div>');
    },
      eventClick: function (calEvent, jsEvent, view) {
          var flwa = 0;
          if (warehouse == "01") {
              if (calEvent.Warehouse == "02") {
                  flwa = 1;
              } else {
                  flwa = 0;
              }
          } else {
              flwa = 0;
          }

          if (flwa == 1) {
              if (calEvent.isfinished == "Y") {

              } else {
                  var drivername = calEvent.driver_WHS;
                  var truckname = calEvent.truck_WHS

                  var id_route = calEvent.title.substr(0, calEvent.title.indexOf(' '));
                  $("#selectedRouteOtraBodega").val(id_route);

                  if (drivername != "") {
                      $("#lstDriverOtraBodega option:contains(" + drivername + ")").attr("selected", true).trigger('change');
                  }
                  if (truckname != "") {
                      $("#lstTruckOtraBodega option:contains(" + truckname + ")").attr("selected", true).trigger('change');
                  }




                  $('#editModal_KY').modal('show');
              }

          } else {
              //Llamamos con AJAX

              if (flagbtn == 0) {
                  flagbtn = 1;
                  var id_route = calEvent.title.substr(0, calEvent.title.indexOf(' '));
                  $("#selectedOrdersTitle").html("Selected Orders for: " + calEvent.title);
                  var date = new Date(calEvent.start);
                  var theDiv = document.getElementById("activity_purchases"); //PARA SALES ORDERS YA SELECCIONADAS
                  var theDiv2 = document.getElementById("activity_emails"); //PARA INVOICES YA SELECCIONADAS
                  var theDivReps = document.getElementById("sortable1");
                  theDiv.innerHTML = "";
                  theDiv2.innerHTML = "";
                  theDivReps.innerHTML = "";
                  //Guardamos en server

                  $.ajax({
                      url: '/Invoices/GetSalesOrdersNew',
                      type: 'POST',
                      datatype: 'application/json',
                      contentType: 'application/json',
                      data: JSON.stringify({
                          id: id_route,
                          date: date
                      }),
                      cache: false,
                      global: false,
                      success: function (result) {
                          $("#loading").hide();
                          $("#selectedRoute").val(id_route);
                          $("#selectedRoute2").val(id_route);
                          $("#nombreruta").html(calEvent.title);

                          var count = 0;


                          var url = '/Invoices/Planning_order/' + id_route;
                          //url = url.replace('ID_ACT', id_route);
                          $("#Sortbtn").attr("href", url)
                          let test = [];
                          if ($.parseJSON(result.result).length) {
                              var arr = $.parseJSON(result.result);
                              var flags = [], output = [], l = arr.length, i;
                              for (i = 0; i < l; i++) {
                                  if (flags[arr[i].CardCode]) continue;
                                  flags[arr[i].CardCode] = true;
                                  output.push(arr[i].CardCode);
                              }

                          
                              $("#eachdata").html(0);
                              $("#casedata").html(0);
                              $("#packdata").html(0);
                              $("#lbsdata").html(0);
                              $("#totalcasesdata").html(0);
                              $("#lblcustomers").html(calEvent.customerscount);
                              $("#ordersdata").html(arr.length);
                              $("#amountdata").html("$" + formatterNoCurrency.format(arr.sum("OpenAmount")));

                             

                          } else {
                              theDiv.innerHTML = "<label>No data to show</label>";



                          }
            
                          $.each($.parseJSON(result.result), function (i, selSO) {
                              var contents = '';
                              var cCustomer = String(selSO.CardCode).charAt(0);
                    
                              contents += '<div class="list-group-item list-group-item-action d-flex align-items-center ">';
                              //contents += '<div class="avatar avatar-xs mr-3">';
                              //contents += '<span class="avatar-title rounded-circle ">';
                              //contents += '<i class="material-icons">description</i>';
                              //contents += '</span>';
                              //contents += '</div>';
                              contents += '<div class="flex">';
                              contents += '<div class="d-flex align-items-middle">';
                              contents += '<strong class="text-15pt mr-1">' + selSO.CardCode + " -" + selSO.CustomerName + '</strong>';
                              contents += '</div>';
                              contents += '<small>' + selSO.SalesPerson + '</small><br>';

                              contents += '<small class="text-muted">' + (new Date(parseInt(selSO.SODate.substr(6)))).toLocaleDateString('en-US') + '</small>';
                              contents += '</div>';
                              contents += '<div class="flex">';
                              contents += '<div class="d-flex align-items-middle">';
                              contents += '<strong class="text-15pt mr-1">#' + selSO.NumSO + '</strong>';
                              contents += '</div>';
                              contents += '<small class="">$' + formatterNoCurrency.format(selSO.OpenAmount) + '</small><br />';
                              contents += '<small class="text-muted">' + selSO.Remarks + '</small>';
                              contents += '</div>';
                              contents += '<div class="avatar avatar-sm mr-3">';
                              contents += '<span class="avatar-title rounded-circle">';

                              if (cCustomer == "C") {
                                  contents += '<img src="../Content/assets/images/fondolimena1.jpg" alt="Avatar" class="avatar-img rounded-circle">';
                              } else {
                                  contents += '<img src="../Content/assets/images/marker/bars.png" alt="Avatar" class="avatar-img rounded-circle">';
                              }
                              
                              contents += '</span>';

                              contents += '</div>';
                              contents += '</div>';

                              theDiv.innerHTML += contents;
                          });
                          //Invoices
                          if ($.parseJSON(result.result13).length) {
                              var arr2 = $.parseJSON(result.result13);
                              $("#invoicedata").html(arr2.length);
                          } else {
                              theDiv2.innerHTML = "<label>No data to show</label>";
                          }

                          $.each($.parseJSON(result.result13), function (i, selSO) {
                              var contents = '';
                              var cCustomer = String(selSO.CardCode).charAt(0);
                    
                              contents += '<div class="list-group-item list-group-item-action d-flex align-items-center ">';
                              //contents += '<div class="avatar avatar-xs mr-3">';
                              //contents += '<span class="avatar-title rounded-circle ">';
                              //contents += '<i class="material-icons">description</i>';
                              //contents += '</span>';
                              //contents += '</div>';
                              contents += '<div class="flex">';
                              contents += '<div class="d-flex align-items-middle">';
                              contents += '<strong class="text-15pt mr-1">' + selSO.CardCode + " -" + selSO.CustomerName + '</strong>';
                              contents += '</div>';
                              contents += '<small>' + selSO.SalesPerson + '</small><br>';

                              contents += '<small class="text-muted">' + (new Date(parseInt(selSO.SODate.substr(6)))).toLocaleDateString('en-US') + '</small>';
                              contents += '</div>';
                              contents += '<div class="flex">';
                              contents += '<div class="d-flex align-items-middle">';
                              contents += '<strong class="text-15pt mr-1">#' + selSO.NumSO + '</strong>';
                              contents += '</div>';
                              contents += '<small class="">$' + formatterNoCurrency.format(selSO.OpenAmount) + '</small><br />';
                              contents += '<small class="text-muted">' + selSO.Remarks + '</small>';
                              contents += '</div>';
                              contents += '<div class="avatar avatar-sm mr-3">';
                              contents += '<span class="avatar-title rounded-circle">';

                              if (cCustomer == "C") {
                                  contents += '<img src="../Content/assets/images/fondolimena1.jpg" alt="Avatar" class="avatar-img rounded-circle">';
                              } else {
                                  contents += '<img src="../Content/assets/images/marker/bars.png" alt="Avatar" class="avatar-img rounded-circle">';
                              }
                              
                              contents += '</span>';

                              contents += '</div>';
                              contents += '</div>';
    
                              theDiv2.innerHTML += contents;
                          });


                          var conf1 = 0;
                          $.each($.parseJSON(result.result6), function (i, conf) {
                              if (conf.isfinished == true || conf.transf > 0) {
                                  conf1 = 1;
                               
                                  $("#alert_routeclosed").show();
                                  $("#newExtra").hide();
                                  $("#Sortbtn").hide();
                      
                              } else {
                                  $("#alert_routeclosed").hide();
                                  $("#newExtra").show();
                                  $("#Sortbtn").show();
                               
                              }
                          });


                          if (conf1 == 0) {


                              $("#btneditr").show();
                              $("#delso").show();
                              $("#savso").show();

                              $("#neworder").show();
                              $("#divopenso").show();
                              $('#divselos').addClass('col-md-6').removeClass('col-md-12');

                          } else {

                              $("#btneditr").hide();
                              $("#delso").hide();
                              $("#savso").hide();
                              $("#neworder").hide();
                              $("#divopenso").hide();
                              $('#divselos').addClass('col-md-12').removeClass('col-md-6');
                          }



                          $('#modal-right').modal('show');
   
                          $('#tab1').get(0).click();
                          //$('#modal-right').modal('show');
                          flagbtn = 0;


                          //Solicitamos otros datos Getextradetails
                          $.ajax({
                              url: '/Invoices/Getextradetails',
                              type: 'POST',
                              datatype: 'application/json',
                              contentType: 'application/json',
                              data: JSON.stringify({
                                  id: id_route
                              }),
                              cache: false,
                              global: false,
                              success: function (result) {
                                  $("#eachdata").html(result.totalEach);
                                  $("#casedata").html(result.totalCases);
                                  $("#packdata").html(result.totalPack);
                                  $("#lbsdata").html(result.totalLBS);
                                  $("#totalcasesdata").html(result.grantotal);
                              },
                              error: function (request) {
                                  $("#eachdata").html(0);
                                  $("#casedata").html(0);
                                  $("#packdata").html(0);
                                  $("#lbsdata").html(0);
                                  $("#totalcasesdata").html(0);
                              }

                          });

                      },
                      error: function (request) {
                          flagbtn = 0;


                      }
                  });
              }
          }



          // change the border color just for fun
          $(".fc-h-event").css("border-color", "#eef2f5 !important");
          $(".fc-h-event").css("background", "#eef2f5");
          $(this).css("background", "#fab63f");
                        //$(this).css("border-color", "#fab63f");


      //$('#newEvent').modal('show');
      //e = event;
      //getEvent(event);
      },
      eventDrop: function (event, delta, revertFunc) {
              revertFunc();
      },
      eventResize: function (event, delta, revertFunc) {
    
              revertFunc();

      },
      eventDragStart: function (event) {
         
      },
        };

    Date.prototype.withoutTime = function () {
        var d = new Date(this);
        d.setHours(0, 0, 0, 0);
        return d;
    }
    var originalDate;
  function setupEvents(){
    $(document).on('click', '#dayview', function() {
      calendar.fullCalendar('changeView', 'agendaDay');
      sr.sync();
    });

    $(document).on('click', '#weekview', function() {
        calendar.fullCalendar('changeView', 'basicWeek');
      sr.sync();
    });

    $(document).on('click', '#monthview', function() {
      calendar.fullCalendar('changeView', 'month');
      sr.sync();
    });

    $(document).on('click', '#todayview', function() {
      calendar.fullCalendar('today');
      sr.sync();
    });

    $(document).on('click', '#btn-new', function() {
      $('#newEvent').modal('show');
      e = {title:'', description:'', start:moment(), end:moment().add(4, 'h'), participant:'1', type:'', className: 'block b-t b-t-2x b-primary'};
      getEvent(e);
    });

    $(document).on('click', '#btn-save', function() {
      var e = getEvent();
      if(e.id){
        calendar.fullCalendar( 'updateEvent', e );
      }else{
        e.id = moment().toDate();
        calendar.fullCalendar( 'renderEvent', e );
      }
      $('#newEvent').modal('hide');
    });
  }

    Array.prototype.sum = function (prop) {
        var total = 0
        for (var i = 0, _len = this.length; i < _len; i++) {
            total += this[i][prop]
        }
        return total
    }

  function getEvent(event){
    var el_title = $('#event-title'),
        el_desc = $('#event-desc'),
        el_type = $('#event-type'),
        el_start_date = $('#event-start-date'),
        el_start_time = $('#event-start-time'),
        el_end_date = $('#event-end-date'),
        el_end_time = $('#event-end-time'),
        el_participant = $('#event-participant');
    // set date to form
    if(event){
      el_title.val(event.title);
      el_desc.val(event.description);

      el_type.find('input[value="'+event.type+'"]').prop('checked', true);
      //el_participant.html( getParticipant(event, 32) );
      el_start_date.val(moment(event.start).format("YYYY-MM-DD"));
      el_start_time.val(moment(event.start).format("HH:mm"));
      el_end_date.val(moment(event.end).format("YYYY-MM-DD"));
      el_end_time.val(moment(event.end).format("HH:mm"));
    // get data from form
    }else{
      e.title = el_title.val();
      e.type = el_type.find('input:checked').val();
      e.start = moment(el_start_date.val() +' '+ el_start_time.val());
      e.end = moment(el_end_date.val() +' '+ el_end_time.val());
      e.description = el_desc.val();
      }

      if (event.editable) {
          $("#btneliminarActividad").show();
          $("#eventidt").val(event.id);
      } else {
  
          $("#btneliminarActividad").hide();
      }

    return e;
  }

  //function getParticipant(event, size){
  //  var participant = '';
  //  var size = size || 24;
  //  $.each(event.participant.split(','), function (index, value) {
  //    participant += '<a href="#" class="avatar w-'+size+'"><img src="../assets/img/a'+value+'.jpg"></a>';
  //  });
  //  return participant;
  //}

    var init = function () {
        var data = eventsCalendar;
 //   var data=[
 //   {
 //     "id": 1,
 //     "type": "Meeting",
 //     "title": "New Project Kick-off meeting",
 //     "description": "Augue onec eleifend nisl eu",
 //     "className": "block b-t b-t-2x b-primary",
 //     "participant": "1,2,4,7,9"
 //   },
 //   {
 //     "id": 2,
 //     "type": "Appointment",
 //     "title": "Meet Mike at company office",
 //     "description": "Phasellus at ultricies neque augue",
 //     "className": "block b-t b-t-2x b-info",
 //     "participant": "4,5,7,8"
 //   },
 //   {
 //     "id": 3,
 //     "type": "Meeting",
 //     "title": "Travel IOS application mockup design",
 //     "description": "Quis malesuada augue",
 //     "className": "block b-t b-t-2x b-success",
 //     "participant": "6,7"
 //   },
 //   {
 //     "id": 4,
 //     "type": "Appointment",
 //     "title": "Weekly report meeting",
 //     "description": "Donec eleifend nisl eu consectetur.",
 //     "className": "block b-t b-t-2x b-danger",
 //     "participant": "3,6"
 //   },
 //   {
 //     "id": 5,
 //     "type": "Meeting",
 //     "title": "All day Meeting",
 //     "description": "Malesuada augue onec eleifend nisl eu.",
 //     "className": "block b-t b-t-2x b-warning",
 //     "participant": "2,6,8,9"
 //   },
 //   {
 //     "id": 6,
 //     "type": "Meeting",
 //     "title": "Repeating Meeting",
 //     "description": "Donec eleifend nisl eu consectetur.",
 //     "className": "block b-t b-t-2x b-primary",
 //     "participant": "6,9"
 //   }
 //];

      // make up the start / end date
        $.each(data, function (index, item) {

            item.start = new Date(parseInt(item.start.substr(6)));
            item.start = new Date(parseInt(item.departure.substr(6)));
          //item.end = new Date(parseInt(item.end.substr(6)));

      });
      option.events = data;
      calendar = $('#fullcalendar').fullCalendar(option);

      //sr.reveal('#fullcalendar', {viewFactor: 0, delay: 10, origin: 'left', distance:'100vw', scale: 1});
      //sr.reveal('.fc-event', {
      //  viewFactor: 0, 
      //  delay: 500,
      //  afterReveal:function (el) {
      //    $(el).css('transform', 'none');
      //    if(!noticed){
      //      notie.alert({ text: 'Start on "Add Event" button', position: 'top' });
      //      noticed = true;
      //    }
      //  }
      //}, 50);

      setupEvents();
   
  }

  // for ajax to init again
  $.fn.fullCalendar.init = init;

})(jQuery);

var formatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
    // the default value for minimumFractionDigits depends on the currency
    // and is usually already 2
});

var formatterNoCurrency = new Intl.NumberFormat('en-US', {
    minimumFractionDigits: 2,
    // the default value for minimumFractionDigits depends on the currency
    // and is usually already 2
});