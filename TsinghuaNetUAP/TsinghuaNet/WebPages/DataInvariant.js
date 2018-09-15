var LocString = String(window.document.location.href);

function getQueryStr(str) {
    var rs = new RegExp("(^|)" + str + "=([^&]*)(&|$)", "gi").exec(LocString);
    if (rs) {
        return rs[2];
    }
    // parameter cannot be found
    return "";
}

var id = getQueryStr("id");
var pw = getQueryStr("pw");

var data = {
    list: [
        {
            name: '网络学堂',
            id: "learn",
            uri: "https://learn.tsinghua.edu.cn/MultiLanguage/lesson/teacher/loginteacher.jsp?userid=" + id + "&userpass=" + pw
        },
        {
            name: '信息门户',
            id: "info",
            uri: "https://info.tsinghua.edu.cn:443/Login?userName=" + id + "&password=" + pw
        },
        {
            name: '信息化服务',
            id: "its",
            uri: "http://its.tsinghua.edu.cn"
        },
        {
            name: '校外访问',
            id: "sslvpn",
            uri: "https://sslvpn.tsinghua.edu.cn/dana-na/auth/url_default/welcome.cgi"
        }
    ]
};

function setLocalName(id, name) {
    data.list.find(function (item) { return item.id === id; }).name = name;
}