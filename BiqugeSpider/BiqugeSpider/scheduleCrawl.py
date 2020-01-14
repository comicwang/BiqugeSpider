# -*- coding: utf-8 -*-
import schedule
import subprocess
import datetime
import time
import logging
from multiprocessing import Process
from scrapy import cmdline

def crawl_work():
    print("I'm working...")
    print('-'*100)
    #爬取策略 p-关键字爬取 r-最新数据爬取  c-类别爬取 （1-玄幻 2-武侠 3-都市 4-历史 6-网游 7-科幻 8-恐怖 10-其他）
    args = ["scrapy", "crawl", 'BiqugeSpider','-a','c=8']
    while True:
        start = time.time()
        p = Process(target=cmdline.execute, args=(args,))
        p.start()
        p.join()
        logging.debug("### use time: %s" % (time.time() - start))

if __name__=='__main__':
    print('*'*10+'开始执行定时爬虫'+'*'*10)
    schedule.every(10).minutes.do(crawl_work)  
    print('当前时间为{}'.format(datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')))
    print('*' * 10 + '定时爬虫开始运行' + '*' * 10)
    while True:
        schedule.run_pending()
        time.sleep(10)
