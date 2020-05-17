# turning
联调工具。



#### 简介

在开发/调试过程中，经常会遇到修改配置文件的问题，其中之一便是应用请求的服务器地址。

像这种直接修改配置文件的？

```json
{
    "server": {
        //"host": "192.168.1.1",
        //"port": 8000
        "host":"prod.com",
        "port":"8888"
    }
}
```

像这样的配置多个环境的？

```json
{
    "prod": {
        "server": {
            .....
        }
    },
    "dev": {
        "server": {
            ....
        }
    }
}
```

还是……其他的什么方案？

**不用怕，使用turning，让你的调试更方便。**

原理简单，作用可不小。

![img1](https://github.com/LorraineMetz/turning/blob/master/files/img1.png)

(这图画的好low)

你可以随时自由的切换你的目标服务器，再也不需要重启应用加载配置。



#### 配置需求

操作系统： Windows 10

浏览器：Microsoft Edge(chromium)



#### 使用

打开软件后，通过新建按钮，新建一组映射，请注意，输入流量必须为域名形式的(不能为ip)。

![img2](https://github.com/LorraineMetz/turning/blob/master/files/img1.png)

添加完这样一组映射，比如：In = http://abc.com Out = http://192.168.1.2:8901

你对abc.com:80的请求，就会被转发到192.168.1.2:8901上。

