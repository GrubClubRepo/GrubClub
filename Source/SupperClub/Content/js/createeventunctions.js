$(function(){
    var index=1;
    //Add, Save, Edit and Delete functions code
    $(".btnEdit").bind("click", Edit);
    $(".btnDelete").bind("click", Delete);
    $(".btnSave").bind("click", Save);
    $("#btnAdd").bind("click", Add);
});
