# -*- coding:utf-8 -*-
import pymssql
#import numpy as np

class Configration:
    """数据库设置"""
    host="DESKTOP-EJQLN15\MSSQLSERVER01"
    port=1433
    user="sa"
    pwd="123456"
    db="GDSSO"
    def __init__(self):
       pass

class UniqueObject(Configration):  #only offer a support of connecting db
    """description of class"""
    cur=None
    conn=None

    def __init__(self):
        Configration.__init__(self)
    def __del__(self):
           if  self.conn!=None:
                      self.conn.close()
                      print(">>>>>>>>>>>>>>>>>Connection has been closed!<<<<<<<<<<<<<<<<<<<")
    @staticmethod
    def GetObject():
        if UniqueObject.cur==None:
            print(">>>>>>>>>>>>>>>>>Connecting to Database.....<<<<<<<<<<<<<<<<<<")
            return UniqueObject.__GetConnect()
        return UniqueObject.conn,UniqueObject.cur

    def __GetConnect():
           if not Configration.db:  
               UniqueObject.conn =pymssql.connect(host=Configration.host,port=Configration.port,
                      user=Configration.user,password=Configration.pwd)
           else:
               UniqueObject.conn =pymssql.connect(host=Configration.host,port=Configration.port,
                      user=Configration.user,password=Configration.pwd,
                      database=Configration.db,charset="utf8")
           UniqueObject.cur=UniqueObject.conn.cursor()
           if not UniqueObject.cur:    raise(NameError,"Connection error!")
           else: return UniqueObject.conn,UniqueObject.cur


class MSSQL():
    conn,cur=UniqueObject.GetObject()
    def __init__(self):
       pass

    def __del__(self):
     #print("\n  MSSQL object has been realsed!")   
      pass
    def GetQuery(self,sql): 
        # MSSQL.cur.execute(sql)
        # reslist=self.cur.fetchall()
        # return reslist
        #cur = conn.cursor()
        result = []
        try:
            MSSQL.cur.execute(sql)
            index = MSSQL.cur.description
            for res in MSSQL.cur.fetchall():
                row = {}
                for i in range(len(index)):
                    row[index[i][0]] = res[i]
                result.append(row)
            #MSSQL.cur.close()
        except:
            print(">>>>>>>>>>>>>>>>>GetQuery exception.....<<<<<<<<<<<<<<<<<<")
        return result

    #带分页查询
    def GetQueryPage(self,sql): 
        result = []
        jieguo = {}
        try:
            MSSQL.cur.execute(sql)
            index = MSSQL.cur.description
            for res in MSSQL.cur.fetchall():
                row = {}
                for i in range(len(index)):
                    row[index[i][0]] = res[i]
                result.append(row)
                jieguo = { "datalist":result}
            #2.下一个结果集
            if MSSQL.cur.nextset():
                result1 = []
                index = MSSQL.cur.description
                for res in MSSQL.cur.fetchall():
                    row = {}
                    for i in range(len(index)):
                        row[index[i][0]] = res[i]
                    result1.append(row)
                    jieguo = { "datalist":result,"datapage":result1}
            #MSSQL.cur.close()
        except:
            print(">>>>>>>>>>>>>>>>>GetQueryPage exception.....<<<<<<<<<<<<<<<<<<")
        return jieguo

    def ExecQuery(self,sql): 
        try:
            self.cur.execute(sql)
            result = self.cur.rowcount
            self.conn.commit()
            return result
        except Exception as e:
            print("\nInserting exception!")
            return -1