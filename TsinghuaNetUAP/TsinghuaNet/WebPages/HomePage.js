
var LocString = String(window.document.location.href);
function getQueryStr(str) {
    var rs = new RegExp("(^|)" + str + "=([^&]*)(&|$)", "gi").exec(LocString), tmp;
    if (tmp = rs) {
        return tmp[2];
    }
    // parameter cannot be found
    return "";
}
var id = getQueryStr("id");
var pw = getQueryStr("pw");
document.getElementById("learning").href = "https://learn.tsinghua.edu.cn/MultiLanguage/lesson/teacher/loginteacher.jsp?userid=" + id + "&userpass=" + pw;
document.getElementById("info").href = "https://info.tsinghua.edu.cn:443/Login?userName=" + id + "&password=" + pw;