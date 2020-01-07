# -*- coding: utf-8 -*-

# Define here the models for your scraped items
#
# See documentation in:
# https://doc.scrapy.org/en/latest/topics/items.html

import scrapy
import uuid
import datetime
from MySqlComment import MySqlComment

class BiqugespiderItem(scrapy.Item):
    # define the fields for your item here like:
    # name = scrapy.Field()
    Title=scrapy.Field()
    Content=scrapy.Field()
    Id=scrapy.Field()
    Chapter=scrapy.Field()

