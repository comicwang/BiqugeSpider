#!/usr/bin/env python
# coding=utf-8

import pymysql

class MySqlComment():
    host='cd-cdb-adv1bzus.sql.tencentcdb.com'
    port=62650
    user='root'
    password='ww313345335'
    defaultDb='Biquge'

    def __init__(self, *args, **kwargs):

        return super().__init__(*args, **kwargs)
    #执行sql语句
    def ExecuteSql(self,sql,len):
         conn=pymysql.connect(host=self.host,port=self.port,user=self.user,password=self.password,database=self.defaultDb)
         cursor=conn.cursor()
         cursor.execute(sql,len)
         conn.commit()
         cursor.close()
         conn.close()

    def Query(self,sql):
        result = []
        conn=pymysql.connect(host=self.host,port=self.port,user=self.user,password=self.password,database=self.defaultDb)
        cursor=conn.cursor()
        cursor.execute(sql)
        result= cursor.fetchall()      
        return  result

    #初始化数据库
    def IniMysql(self):
         #创建数据库
      conn=pymysql.connect(host=self.host,port=self.port,user=self.user,password=self.password,database='mysql')
      sql='create database if not exists Biquge'
      cursor=conn.cursor()
      cursor.execute(sql)
      cursor.close()
      conn.close()           
      #创建表
      conn=pymysql.connect(host=self.host,port=self.port,user=self.user,password=self.password,database=self.defaultDb)
      cursor=conn.cursor()
      sql= """CREATE TABLE IF NOT EXISTS BookBasic(
                    BookName varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '书名',
                    Author varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '作者',
                    Image longblob NULL COMMENT '书封面',
                    LatestChapter varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '最新章节',
                    Desc1 varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '内容描述',
                    Id char(36) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '主键',
                    LatestTime datetime(0) NULL DEFAULT NULL COMMENT '最近更新时间',
                    Catelog varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '分类',
                    PRIMARY KEY (`Id`) USING BTREE
                  ) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Dynamic;
              """   
      cursor.execute(sql)
      sql="""CREATE TABLE IF NOT EXISTS BookContent(
                    DId char(36) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
                    Title varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '详情标题',
                    Content longtext CHARACTER SET utf8 COLLATE utf8_general_ci NULL COMMENT '内容信息',
                    Id char(36) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '书主键',
                    Chapter varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT NULL COMMENT '章节',
                    SyncTime datetime(0) NULL DEFAULT NULL COMMENT '入库时间',
                    PRIMARY KEY (`DId`) USING BTREE
                  ) ENGINE = InnoDB CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Compact;
          """
      cursor.execute(sql)
      cursor.close()
      conn.close()      