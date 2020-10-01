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
                  var theDiv = document.getElementById("sortable2"); //PARA SALES ORDERS YA SELECCIONADAS
                  var theDivReps = document.getElementById("sortable1");
                  theDiv.innerHTML = "";
                  theDivReps.innerHTML = "";
                  //Guardamos en server

                  $.ajax({
                      url: '/Invoices/GetSalesOrders',
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

                          var count = 0;


                          var url = '/Invoices/Planning_order/' + id_route;
                          //url = url.replace('ID_ACT', id_route);
                          $("#Sortbtn").attr("href", url)

                          var contents = '';

                          $.each($.parseJSON(result.result), function (i, selSO) {
                              count++;
                              var cCustomer = String(selSO.CardCode).charAt(0);

                              contents = '<li id="' + selSO.NumSO + '">';

                              contents += '<div class="media">';
                              if (cCustomer == "C") {
                                  contents += '<div class="badge badge-warning pillso">' + selSO.NumSO + '</div>';
                              } else {
                                  contents += '<div class="badge badge-info pillso">' + selSO.NumSO + '</div>';
                              }

                              contents += '<div class="media-body list-widget-border"><div class="float-left">';
                              //contents += '<h6 class="text-uppercase">' + selSO.CardCode + " -" + selSO.CustomerName + '</h6>';
                              contents += '<span style="font-size:12px;"  class="pillcc2"><strong>' + selSO.CardCode + "" + '  -  </strong></span><label style="font-size:12px;" class="text-uppercase pillCustomer2 text-black"><strong>' + selSO.CustomerName + '</strong></label><br>';
                              contents += '<small class="text-uppercase">Route: ' + selSO.DeliveryRoute + '</small><br/>';
                              contents += '<small class="text-uppercase">Sales Rep: ' + selSO.SalesPerson + '</small><br />';
                              //contents += '<small class="text-uppercase">Printed: ' + selSO.Printed + '</small><br />';

                              contents += '<small>Amount: $ </small><small class="pillAmount2">' + selSO.OpenAmount.toFixed(2) + '</small><br>';
                              //contents += '<div class="float-right">';
                              //contents += '<br><small class="text-muted text-sm">Date:' + (new Date(parseInt(selSO.SODate.substr(6)))).toLocaleDateString('en-US')  + '</small>';
                              contents += '<small class="text-muted text-sm pilldate2">Date:' + (new Date(parseInt(selSO.SODate.substr(6)))).toLocaleDateString('en-US', { timeZone: 'UTC' }) + '</small>';
                              if (selSO.Remarks === null) {

                              } else {
                                  if (selSO.Remarks != "") {
                                      contents += '<br><p class="text-muted" title="' + selSO.Remarks + '">Remarks</p>';
                                  }
                              }

                              contents += '</div></div></div>';

                              contents += '</li>';

                              theDiv.innerHTML += contents;
                          });

                          var contentOpenSO = "";  //PARA SALES ORDERS ABIERTAS

                          $.each($.parseJSON(result.result2), function (i, openSO) {
                              var cCustomer2 = String(openSO.CardCode).charAt(0);
                              contentOpenSO = '<li id="' + openSO.NumSO + '">';

                              contentOpenSO += '<div class="media">';
                              if (cCustomer2 == "C") {
                                  contentOpenSO += '<div class="badge badge-warning pillso">' + openSO.NumSO + '</div>';
                              } else {
                                  contentOpenSO += '<div class="badge badge-info pillso">' + openSO.NumSO + '</div>';
                              }

                              contentOpenSO += '<div class="media-body list-widget-border"><div class="float-left">';
                              contentOpenSO += '<span style="font-size:12px;" class="pillcc2"><strong>' + openSO.CardCode + "" + '  -  </strong></span><label style="font-size:12px;" class="text-uppercase pillCustomer2 text-black"><strong>' + openSO.CustomerName + '</strong></label><br>';
                              contentOpenSO += '<small class="text-uppercase pillRoute">Route: ' + openSO.DeliveryRoute + '</small><br />';
                              contentOpenSO += '<small class="text-uppercase">Sales Rep: ' + openSO.SalesPerson + '</small><br />';
                              if (openSO.Printed == "Y") {
                                  contentOpenSO += '<small class="text-uppercase">Printed: ' + "YES" + '</small><br />';
                              } else {
                                  contentOpenSO += '<small class="text-uppercase">Printed: ' + "NO" + '</small><br />';
                              }

                              if (openSO.OpenAmount == openSO.TotalSO) {
                                  contentOpenSO += '<small>Amount: $ </small><small class="pillAmount2">' + openSO.OpenAmount.toFixed(2) + '</small><br>';

                              } else {
                                  contentOpenSO += '<small>Amount: $ </small><small class="pillAmount2">' + openSO.OpenAmount.toFixed(2) + '</small><small class="text-muted" style="font-size:10px;">   -   Total SO: $ ' + openSO.TotalSO.toFixed(2) + '</small><br>';

                              }


                              //contentOpenSO += '<div class="float-right">';
                              //contentOpenSO += '<br/><small class="text-muted">Date: ' + (new Date(parseInt(openSO.SODate.substr(6)))).toLocaleDateString('en-US') + '</small>';
                              contentOpenSO += '<small class="text-muted pilldate2">Date: ' + (new Date(parseInt(openSO.SODate.substr(6)))).toLocaleDateString('en-US', { timeZone: 'UTC' }) + '</small>';
                              if (openSO.Remarks === null) {

                              } else {
                                  if (openSO.Remarks != "") {
                                      contentOpenSO += '<br><p class="text-muted" title="' + openSO.Remarks + '">Remarks</p>';
                                  }
                              }
                              contentOpenSO += '</div></div></div>';

                              contentOpenSO += '</li>';
                              theDivReps.innerHTML += contentOpenSO;
                          });


                          var conf1 = 0;
                          $.each($.parseJSON(result.result6), function (i, conf) {
                              if (conf.isfinished == true || conf.transf > 0) {
                                  conf1 = 1;
                                  alert("Esta ruta ya fue cerrada o cuenta con transferencias de productos");
                              }
                          });


                          if (conf1 == 0) {
                              $("#sortable1").sortable({
                                  connectWith: ".connectedSortable"
                              }).disableSelection();


                              $("#sortable2").sortable({
                                  connectWith: ".connectedSortable",
                                  update: function () {
                                      var column2RelArray = [];
                                      $('ul#sortable2 small.pillAmount2').each(function () {
                                          column2RelArray.push($(this).text());
                                      });

                                      const result = column2RelArray.reduce((r, e) => r + +e.replace(',', '.'), 0)


                                      $("#totalAmountSO").html("Total Amount: " + formatter.format(result.toFixed(2)));
                                      $("#totalSO").html("Total Sales Orders: " + column2RelArray.length);

                                      ///

                                      var columnCustomerRelArray = [];
                                      $('ul#sortable2 label').each(function () {
                                          columnCustomerRelArray.push($(this).text());
                                          $.unique(columnCustomerRelArray.sort());
                                      });
                                      $("#totalCustomers").html("Total Customers: " + columnCustomerRelArray.length);

                                      $("#totalExtra").html("Total Extra: $" + result.result7);


                                  }
                              }).disableSelection();


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

                          //$(function () {
                          //    $("#sortable1, #sortable2").sortable({
                          //        connectWith: ".connectedSortable",
                          //        update: function () {
                          //            var column2RelArray = [];
                          //            $('ul#sortable2 small.pillAmount').each(function () {
                          //                column2RelArray.push($(this).text());
                          //            });
                          //            console.log(column2RelArray);
                          //        }

                          //    }).disableSelection();


                          //});

                          //$("body").tooltip({ selector: '[data-toggle=tooltip]' });


                          /* ---------------------------------------------
                           Configure popover globally
                           --------------------------------------------- */
                          //$('[data-toggle="popover"]').popover();





                          var column3RelArray = [];
                          $('ul#sortable2 small.pillAmount2').each(function () {
                              column3RelArray.push($(this).text());
                          });

                          const result2 = column3RelArray.reduce((r, e) => r + +e.replace(',', '.'), 0)


                          $("#totalAmountSO").html("Total Amount: " + formatter.format(result2.toFixed(2)));
                          $("#totalSO").html("Total Sales Orders: " + column3RelArray.length);

                          $("#totalExtra").html("Total Extra: $" + result.result7);

                          ///
                          var columnCustomerRelArray2 = [];
                          $('ul#sortable2 label').each(function () {
                              columnCustomerRelArray2.push($(this).text());
                              $.unique(columnCustomerRelArray2.sort());
                          });

                          $("#totalCustomers").html("Total Customers: " + columnCustomerRelArray2.length);

                          //
                          $("#officeFltr").html('');
                          var o = new Option("option text", "0");
                          /// jquerify the DOM object 'o' so we can use the html method
                          $(o).html("SELECT ALL ROUTES...");
                          $("#officeFltr").append(o);
                          $.each($.parseJSON(result.result3), function (i, routes) {
                              var o = new Option("option text", "value");
                              /// jquerify the DOM object 'o' so we can use the html method
                              $(o).html(routes);
                              $("#officeFltr").append(o);
                          });

                          //REPS
                          $("#officeFltr2").html('');
                          var o = new Option("option text", "0");
                          /// jquerify the DOM object 'o' so we can use the html method
                          $(o).html("SELECT ALL REPS...");
                          $("#officeFltr2").append(o);
                          $.each($.parseJSON(result.result4), function (i, reps) {
                              var o = new Option("option text", "value");
                              /// jquerify the DOM object 'o' so we can use the html method
                              $(o).html(reps);
                              $("#officeFltr2").append(o);
                          });

                          ///
                          //CUSTOMER
                          $("#officeFltr3").html('');
                          var o = new Option("option text", "0");
                          /// jquerify the DOM object 'o' so we can use the html method
                          $(o).html("SELECT ALL CUSTOMERS...");
                          $("#officeFltr3").append(o);
                          $.each($.parseJSON(result.result5), function (i, customer) {
                              var o = new Option("option text", "value");
                              /// jquerify the DOM object 'o' so we can use the html method
                              $(o).html(customer.CustomerName);
                              $("#officeFltr3").append(o);
                          });
                          //



                          //UOM INFO
                          $("#Each").html("Each: " + result.result8);
                          $("#Case").html("Case: " + result.result9);
                          $("#Pack").html("Pack: " + result.result10);
                          $("#LBS").html("LBS: " + result.result11);
                          $("#totalEach").html("Total Cases: " + formatterNoCurrency.format(result.result12));


                          $('#modal_SO').modal('show');
                          //$('#modal-right').modal('show');
                          flagbtn = 0;
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