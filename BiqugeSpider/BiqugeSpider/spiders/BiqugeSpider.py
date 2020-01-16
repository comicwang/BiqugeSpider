# -*- coding: utf-8 -*-
import scrapy
import datetime
import re
import pymysql
import requests
import base64
import uuid
import char2num

from items import BiqugespiderItem
from MySqlComment import MySqlComment

class BiqugeSpider(scrapy.Spider):
    # 爬虫名
    name = "BiqugeSpider"
    # 爬虫作用范围
    allowed_domains = ["www.biduo.cc"]

    domainUrl="https://www.biduo.cc/"

    url = "https://www.biduo.cc/"

    #默认爬取完本小说
    start_urls = ["https://www.biduo.cc/quanbu/"]  

    p=None
    
    r=None

    c=None

    mysql=MySqlComment()

    def __init__(self, name=None,p=None,r=None,c=None,**kwargs):
        if p:
            self.p=p
            self.start_urls[0]="https://www.biduo.cc/search.php?q="+ p;
        if r:
            self.r=r
            self.start_urls[0]="https://www.biduo.cc";
        if c:
            self.c=c
            self.start_urls[0]="https://www.biduo.cc/book_"+ c +"_1/"
        #初始化mysql数据库
        self.mysql.IniMysql()
          
    def parse(self, response):  
        index=0
        #检索小说（这里通过判断关键字来进行全文或者关键字爬取）
        if self.p:
            for each in response.xpath("//*[@class='search-result-page-main']/a"):
               src=each.attrib['href']
               yield scrapy.Request(self.domainUrl+ src, callback = self.ParsePage,dont_filter=False)     
        elif self.r:
            for each in response.xpath("//*[@id='newscontent']/div[2]/ul/li"):
                src=each.xpath(".//span[2]/a")[0].attrib['href']
                yield scrapy.Request(self.domainUrl+ src, callback = self.BookBasic,dont_filter=False) 
        elif self.c:
            for each in response.xpath("//*[@id='newscontent']/div[2]/ul/li"):
                src=each.xpath(".//span[1]/a")[0].attrib['href']
                yield scrapy.Request(self.domainUrl+ src, callback = self.BookBasic,dont_filter=False) 
        else:
            for each in response.xpath("//*[@id='main']/div[1]/ul/li"):
                if index>0:
                    src=each.xpath(".//span[2]/a")[0].attrib['href']
                    yield scrapy.Request(self.domainUrl+ src, callback = self.BookBasic,dont_filter=False) 
                index=index+1

    #获取分页内容，进行关键字深度爬取
    def ParsePage(self,response):
        for each in response.xpath("//*[@class='result-list']/div"):
            src=each.xpath(".//div/a")[0].attrib['href']
            yield scrapy.Request(self.domainUrl+ src, callback = self.BookBasic,dont_filter=False) 

    #获取小说基本信息，同时让调度器爬取小说内容
    def BookBasic(self,response):
        Id=uuid.uuid1()
        BookName=response.xpath("string(//*[@id='info']/h1)")[0].root.strip()  
        LatestTime=response.xpath("string(//*[@id='info']/p[3])")[0].root.strip().split('：')[1]
        Author=response.xpath("string(//*[@id='info']/p)")[0].root.strip().split('：')[1]
        imgSrc=response.xpath("//*[@id='fmimg']/img")[0].attrib['src']
        Catelog=response.xpath("string(//*[@class='con_top']/a[2])")[0].root.strip()
        img= requests.get(imgSrc)
        Image= img.content
        LatestChapter=response.xpath("string(//*[@id='info']/p[4]/a)")[0].root.strip()
        Desc1=response.xpath("string(//*[@id='intro'])")[0].root.strip()
        #查询小说是否存在
        resp= self.mysql.Query("select Id from BookBasic where BookName='{0}'".format(BookName))
        if not resp:          
           # 获取小说信息          
           Id=uuid.uuid1()
           #插入数据
           self.mysql.ExecuteSql("insert into BookBasic (BookName,Author,Image,LatestChapter,Desc1,Id,LatestTime,Catelog) values(%s,%s,%s,%s,%s,%s,%s,%s)",(str(BookName),str(Author),Image,str(LatestChapter),str(Desc1),str(Id),datetime.datetime.strptime(LatestTime, "%Y-%m-%d %H:%M:%S"),str(Catelog)))
        else:
            Id=resp[0][0]
            self.mysql.ExecuteSql("update BookBasic set LatestChapter=%s,Desc1=%s,LatestTime=%s,Catelog=%s where Id=%s",(str(LatestChapter),str(Desc1),datetime.datetime.strptime(LatestTime, "%Y-%m-%d %H:%M:%S"),str(Catelog),str(Id)))
        #查询所有章节
        list=self.mysql.Query("select Title from BookContent where Id='{0}'".format(str(Id)))
        index=0
        #获取当前最大章节数
        maxchapter=self.mysql.Query("select max(num) from (select cast(Chapter as SIGNED) as num from BookContent where id='{0}') as chapter".format(str(Id)))
        if maxchapter[0][0]:
            index= maxchapter[0][0]
        listName=[]
        for each in response.xpath("//*[@id='list']/dl/dd"):
             try:                                             
                 src= each.xpath(".//a")[0].attrib['href']
                 title=each.xpath("string(.//a)")[0].root.strip()
                 if self.InTable(list,title):
                     print(title,"已经入库...")
                 elif title in listName:
                     print(title,"重复章节...")                
                 else:
                     request= scrapy.Request(self.domainUrl+ src, callback = self.BookContent,dont_filter=False)   
                     request.meta['id']=Id
                     request.meta['Chapter']=index
                     index=index+1
                     listName.append(title)
                     yield request
             except Exception as e:
                 print("analysis item error:"+item["title"]+ e);

    #爬取每个章节小说内容
    def BookContent(self,response):
        try:
            item=BiqugespiderItem()
            item['Title']=response.xpath("string(//*[@class='bookname']/h1)")[0].root.strip()
            item['Content']=response.xpath("string(//*[@id='content'])")[0].root.strip()
            item['Id']= response.meta['id']
            item['Chapter']= response.meta['Chapter']
            yield item
        except Exception as e:
            print("login item error:"+item["title"]+ e);

    def InTable(self,list,item):
        for i in list:
            if i[0]==item:
                return True
        return False

    #转换章节
    def ParseChapter(self,title):
        #匹配中文汉字
        chs=title.replace('第','').split('章')
        if chs and len(chs)>1:
            try:
                if chs[0].isdigit():
                    return chs[0]
                else:
                    return char2num.ch2num(chs[0])
            except e:
                pass
        #正则匹配数字
        list= re.findall('\d+',title)
        if list:
            return list[0]
        else:      
            return 0
            