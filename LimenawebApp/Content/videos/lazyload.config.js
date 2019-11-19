// lazyload config
function root() {
    var scripts = document.getElementsByTagName('script'),
        script = scripts[scripts.length - 1],
        path = script.getAttribute('src').split('/'),
        pathname = location.pathname.split('/'),
        notSame = false,
        same = 0;

    for (var i in path) {
        if (!notSame) {
            if (path[i] == pathname[i]) {
                same++;
            } else {
                notSame = true;
            }
        }
    }
    return location.origin + pathname.slice(0, same).join('/');
}

var basepath = root();

var MODULE_CONFIG = {
    chat: [
        '' + basepath + '/Content/BasikTemplate/libs/list.js/dist/list.js',
        '' + basepath + '/Content/BasikTemplate/libs/notie/dist/notie.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/notie.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/app/chat.js'
    ],
    mail: [
        '' + basepath + '/Content/BasikTemplate/libs/list.js/dist/list.js',
        '' + basepath + '/Content/BasikTemplate/libs/notie/dist/notie.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/notie.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/app/mail.js'
    ],
    user: [
        '' + basepath + '/Content/BasikTemplate/libs/list.js/dist/list.js',
        '' + basepath + '/Content/BasikTemplate/libs/notie/dist/notie.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/notie.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/app/user.js'
    ],
    search: [
        '' + basepath + '/Content/BasikTemplate/libs/list.js/dist/list.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/app/search.js'
    ],
    invoice: [
        '' + basepath + '/Content/BasikTemplate/libs/list.js/dist/list.js',
        '' + basepath + '/Content/BasikTemplate/libs/notie/dist/notie.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/app/invoice.js'
    ],
    fullscreen: [
        '' + basepath + '/Content/BasikTemplate/libs/jquery-fullscreen-plugin/jquery.fullscreen-min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/fullscreen.js'
    ],
    jscroll: [
        '' + basepath + '/Content/BasikTemplate/libs/jscroll/dist/jquery.jscroll.min.js'
    ],
    countTo: [
        '' + basepath + '/Content/BasikTemplate/libs/jquery-countto/jquery.countTo.js'
    ],
    stick_in_parent: [
        '' + basepath + '/Content/BasikTemplate/libs/sticky-kit/dist/sticky-kit.min.js'
    ],
    stellar: [
        '' + basepath + '/Content/BasikTemplate/libs/jquery.stellar/jquery.stellar.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/stellar.js'
    ],
    masonry: [
        '' + basepath + '/Content/BasikTemplate/libs/masonry-layout/dist/masonry.pkgd.min.js'
    ],
    owlCarousel: [
        '' + basepath + '/Content/BasikTemplate/libs/owl.carousel/dist/assets/owl.carousel.min.css',
        '' + basepath + '/Content/BasikTemplate/libs/owl.carousel/dist/assets/owl.theme.css',
        '' + basepath + '/Content/BasikTemplate/libs/owl.carousel/dist/owl.carousel.min.js'
    ],
    sort: [
        '' + basepath + '/Content/BasikTemplate/libs/html5sortable/dist/html.sortable.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/sort.js'
    ],
    chartjs: [
        '' + basepath + '/Content/BasikTemplate/libs/moment/min/moment-with-locales.min.js',
        '' + basepath + '/Content/BasikTemplate/libs/chart.js/dist/Chart.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/chartjs.js'
    ],
    chartist: [
        '' + basepath + '/Content/BasikTemplate/libs/chartist/dist/chartist.min.js',
        '' + basepath + '/Content/BasikTemplate/libs/chartist/dist/chartist.min.css',
        '' + basepath + '/Content/BasikTemplate/libs/chartist-plugin-tooltips/dist/chartist-plugin-tooltip.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/chartist.js'
    ],
    dataTable: [
        '' + basepath + '/Content/BasikTemplate/libs/datatables/media/js/jquery.dataTables.min.js',
        '' + basepath + '/Content/BasikTemplate/libs/datatables.net-bs4/js/dataTables.bootstrap4.min.js',
        '' + basepath + '/Content/BasikTemplate/libs/datatables.net-bs4/css/dataTables.bootstrap4.min.css',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/datatable.js'
    ],
    bootstrapTable: [
        '' + basepath + '/Content/BasikTemplate/libs/bootstrap-table/dist/bootstrap-table.min.js',
        '' + basepath + '/Content/BasikTemplate/libs/bootstrap-table/dist/extensions/export/bootstrap-table-export.min.js',
        '' + basepath + '/Content/BasikTemplate/libs/bootstrap-table/dist/extensions/mobile/bootstrap-table-mobile.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/tableExport.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/bootstrap-table.js'
    ],
    bootstrapWizard: [
        '' + basepath + '/Content/BasikTemplate/libs/twitter-bootstrap-wizard/jquery.bootstrap.wizard.min.js'
    ],
    dropzone: [
        '' + basepath + '/Content/BasikTemplate/libs/dropzone/dist/min/dropzone.min.js',
        '' + basepath + '/Content/BasikTemplate/libs/dropzone/dist/min/dropzone.min.css'
    ],
    typeahead: [
        '' + basepath + '/Content/BasikTemplate/libs/typeahead.js/dist/typeahead.bundle.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/typeahead.js'
    ],
    datepicker: [
        '' + basepath + '/Content/BasikTemplate/libs/bootstrap-datepicker/dist/js/bootstrap-datepicker.min.js',
        '' + basepath + '/Content/BasikTemplate/libs/bootstrap-datepicker/dist/css/bootstrap-datepicker.min.css',
    ],
    daterangepicker: [
        '' + basepath + '/Content/BasikTemplate/libs/daterangepicker/daterangepicker.css',
        '' + basepath + '/Content/BasikTemplate/libs/moment/min/moment-with-locales.min.js',
        '' + basepath + '/Content/BasikTemplate/libs/daterangepicker/daterangepicker.js'
    ],
    fullCalendar: [
        '' + basepath + '/Content/BasikTemplate/libs/moment/min/moment-with-locales.min.js',
        '' + basepath + '/Content/BasikTemplate/libs/fullcalendar/dist/fullcalendar.min.js',
        '' + basepath + '/Content/BasikTemplate/libs/fullcalendar/dist/fullcalendar.min.css',
        '' + basepath + '/Content/BasikTemplate/libs/notie/dist/notie.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/notie.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/app/calendar.js'
    ],
    parsley: [
        '' + basepath + '/Content/BasikTemplate/libs/parsleyjs/dist/parsley.min.js'
    ],
    select2: [
        '' + basepath + '/Content/BasikTemplate/libs/select2/dist/css/select2.min.css',
        '' + basepath + '/Content/BasikTemplate/libs/select2/dist/js/select2.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/select2.js'
    ],
    summernote: [
        '' + basepath + '/Content/BasikTemplate/libs/summernote/dist/summernote.css',
        '' + basepath + '/Content/BasikTemplate/libs/summernote/dist/summernote-bs4.css',
        '' + basepath + '/Content/BasikTemplate/libs/summernote/dist/summernote.min.js',
        '' + basepath + '/Content/BasikTemplate/libs/summernote/dist/summernote-bs4.min.js'
    ],
    vectorMap: [
        '' + basepath + '/Content/BasikTemplate/libs/jqvmap/dist/jqvmap.min.css',
        '' + basepath + '/Content/BasikTemplate/libs/jqvmap/dist/jquery.vmap.js',
        '' + basepath + '/Content/BasikTemplate/libs/jqvmap/dist/maps/jquery.vmap.world.js',
        '' + basepath + '/Content/BasikTemplate/libs/jqvmap/dist/maps/jquery.vmap.usa.js',
        '' + basepath + '/Content/BasikTemplate/libs/jqvmap/dist/maps/jquery.vmap.france.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/jqvmap.js'
    ],
    plyr: [
        '' + basepath + '/Content/BasikTemplate/libs/plyrist/src/plyrist.css',
        '' + basepath + '/Content/BasikTemplate/libs/plyrist/src/plyrist.js',
        '' + basepath + '/Content/BasikTemplate/libs/plyr/dist/plyr.css',
        '' + basepath + '/Content/BasikTemplate/libs/plyr/dist/plyr.polyfilled.min.js',
        '' + basepath + '/Content/BasikTemplate/assets/js/plugins/plyr.js'
    ]
};

var MODULE_OPTION_CONFIG = {
    parsley: {
        errorClass: 'is-invalid',
        successClass: 'is-valid',
        errorsWrapper: '<ul class="list-unstyled text-sm mt-1 text-muted"></ul>'
    }
}
