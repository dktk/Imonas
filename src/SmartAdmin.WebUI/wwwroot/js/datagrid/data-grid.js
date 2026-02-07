(function () {
    'use strict';

    function onDblClickRow(index, row) {
    }

    window.$dg = $(gridOptions.datagridContainerSelector).datagrid({
        height: (window.innerHeight - 320),
        method: 'GET',
        rownumbers: true,
        singleSelect: true,
        selectOnCheck: true,
        checkOnSelect: true,
        pagination: true,
        clientPaging: false,
        remoteFilter: true,
        sortName: gridOptions.sortName,
        sortOrder: 'asc',
        pageSize: 20,
        pageList: [20, 50, 100],
        fitColumns: true,
        singleSelect: true,
        selectOnCheck: true,
        queryParams: {},
        
        onDblClickRow: gridOptions.onDblClickRow || onDblClickRow,
        columns: [gridOptions.columns]
    }).datagrid('enableFilter', {})
        .datagrid('load', gridOptions.loadDataUrl);
})();
