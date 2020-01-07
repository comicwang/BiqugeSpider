# -*- coding: utf-8 -*-

# Define your item pipelines here
#
# Don't forget to add your pipeline to the ITEM_PIPELINES setting
# See: https://doc.scrapy.org/en/latest/topics/item-pipeline.html
from MySqlComment import MySqlComment
import uuid
import datetime

class BiqugespiderPipeline(object):
    def process_item(self, item, spider):
        DId=uuid.uuid1()
        SyncTime=datetime.datetime.now()
        mysql=MySqlComment()
        mysql.ExecuteSql("insert into BookContent values(%s,%s,%s,%s,%s,%s)",(str(DId),str(item['Title']),str(item['Content']),str(item['Id']),str(item['Chapter']),SyncTime))
        return item
