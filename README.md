## TinyStoreCore 介绍

使用.net6开发，实现虚拟商品在线销售平台，由平台管理员添加商户，每个商户可以添加多个店铺，不同店铺由自己的独立链接。

实现平台货源店铺设置介个销售，店铺也可以添加自己的货源进行销售，不完全受制于平台的货源种类。

平台管理源可以设置官方收款方式以及给不同商户设置不同的手续费标准。1）商户可以通过系统收款来销售，自动扣除成本和手续费，利润自动计算在商户余额中可以自主提现。2）商户也可以添加自己的收款方式来销售，这种情况下需要商户提前预充值担保签帐金额用于扣除货源成本和手续费。



[![](https://badgen.net/badge/lpyedge/tinystorecore/purple?icon=github)](https://github.com/lpyedge/tinystorecore)
![](https://badgen.net/github/commits/lpyedge/tinystorecore/?icon=github&color=green)
[![](https://badgen.net/github/license/lpyedge/tinystorecore?color=grey)](https://github.com/lpyedge/tinystorecore/blob/main/LICENSE)


### 使用的开源项目
[![](https://badgen.net/badge/donet5/SqlSugar/?icon=github)](https://github.com/donet5/SqlSugar)![](https://badgen.net/github/license/donet5/SqlSugar?color=grey)

[![](https://badgen.net/badge/TinyMapper/TinyMapper/?icon=github)](https://github.com/TinyMapper/TinyMapper)![](https://badgen.net/github/license/TinyMapper/TinyMapper?color=grey)

[![](https://badgen.net/badge/lionsoul2014/ip2region/?icon=github)](https://github.com/lionsoul2014/ip2region)![](https://badgen.net/github/license/lionsoul2014/ip2region?color=grey)

[![](https://badgen.net/badge/2881099/NPinyin/?icon=github)](https://github.com/2881099/NPinyin)

[![](https://badgen.net/badge/bcgit/bc-csharp/?icon=github)](https://github.com/bcgit/bc-csharp)



### 运行

```
docker run -d \
  --name=js.189.cn-speedup \
  --restart unless-stopped \
  lpyedge/js.189.cn-speedup
```