using Oracle.ManagedDataAccess.Client;
using System;
using System.Data.Common;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public class basic_calculate // 基础计算
{
    public static int Check_null(string data)//检测字符串是否为NULL
    {
        //Console.WriteLine(data);
        if(data =="NULL")
        {
            return 1;
        }
        return 0;
    }
    public static string ComputeSHA256(string input)//计算sha-256编码
    {
        // 验证输入
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty");

        // 使用SHA256.Create()创建实例（推荐方式）
        using (SHA256 sha256 = SHA256.Create())
        {
            // 将字符串转换为字节数组（UTF-8编码）
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            // 计算哈希值
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            // 将字节数组转换为十六进制字符串
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}

public class db_insert
{
    public static int Insert_normal(OracleConnection connection)//修改基本信息表
    {
        Console.WriteLine("请输入插入参数：");
        string data = Console.ReadLine();//获取当前参数
        if(data==null)
        {
            return -1;//-1表示参数不正确
        }
        string action = "insert into student(";//action是要执行的sql语言
        string[] option = new string[10];
        int cnt = 0;
        int[] flag = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        string hash_value = basic_calculate.ComputeSHA256(data);//先获取hash值
        string check = "SELECT COUNT(*) FROM student WHERE ID = '" + hash_value + "'";
        int count = 0;
        //Console.WriteLine(check);
        using (OracleCommand command = new OracleCommand(check, connection))
        {

            //ExecuteScalar 返回结果集的第一行第一列的值
            //这里就是 COUNT(*) 的结果，我们将其转换为 int
            object result = command.ExecuteScalar();
            if (result != null && result != DBNull.Value)
            {
                count = Convert.ToInt32(result);
            }
        }
        //Console.WriteLine(count);
        if (count!=0)
        {
            return -2;//-2表示重复插入错误
        }

        for (int i=0;i<data.Length;i++)
        {
            if (data[i]==',')
            {
                if (basic_calculate.Check_null(option[cnt])==0)//如果当前项不为NULL
                {
                    flag[cnt] = 1;//当前位有效
                }
                cnt++;
                if(cnt>8)//超限防溢出
                {
                    return -1;
                }
                continue;
            }
            option[cnt] = option[cnt] + data[i];
        }
        if (basic_calculate.Check_null(option[cnt]) == 0)//如果当前项不为NULL
        {
            flag[cnt] = 1;//当前位有效
        }   
        if (cnt != 8)//防止参数数量不正确
        {
            return -1;
        }
        if (flag[0] == 0)//姓名如果为空
        {
            return -1;//一定不正确
        }
        action = action + "name";
        if (flag[1] == 1)//别名
        {
            action = action + ",nickname";
        }
        if (flag[2] == 1)//性别
        {
            action = action + ",sex";
        }
        if (flag[3] == 1)//学号
        {
            action = action + ",normal_ID";
        }
        if (flag[4] == 1)//生日
        {
            action = action + ",birthday";
        }
        if (flag[5] == 1)//入校时间
        {
            action = action + ",admission_time";
        }
        if (flag[6] == 1)//毕业时间
        {
            action = action + ",graduation_time";
        }
        if (flag[7] == 1)//国籍
        {
            action = action + ",country";
        }
        if (flag[8] == 1)//籍贯
        {
            action = action + ",hometown";
        }
        action = action + ",ID";
        action = action + ")\n";
        action = action + "values(";
        action = action + "'" + option[0]+ "'";
        for(int i=1;i<9;i++)
        {
            if (flag[i]!=0)
            {
                action= action + ",'" + option[i] + "'";
            }
        }
        action = action + ",'" + hash_value + "'";
        action = action + ")\n";
        //Console.WriteLine(action);
        using (OracleCommand command = new OracleCommand(action, connection))
        {
            // 执行 INSERT 语句
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine($"INSERT 操作成功执行，影响了 {rowsAffected} 行。");
        }
        return 0;
    }
}

public class users
{
    public static int Login(string user_name, string password, OracleConnection connection)//登录
    {
        string check = "SELECT COUNT(*) FROM db_users WHERE name = '" + user_name + "'";
        int count = 0;
        using (OracleCommand command = new OracleCommand(check, connection))
        {
            //ExecuteScalar 返回结果集的第一行第一列的值
            //这里就是 COUNT(*) 的结果，我们将其转换为 int
            object result = command.ExecuteScalar();//查询同名用户数量
            if (result != null && result != DBNull.Value)
            {
                count = Convert.ToInt32(result);
            }
        }
        if(count==0)
        {
            return -1;//-1表示未注册错误
        }
        check = "SELECT password FROM db_users WHERE name = '" + user_name + "'";
        string ans="";
        using (OracleCommand command = new OracleCommand(check, connection))
        {
            //ExecuteScalar 返回结果集的第一行第一列的值
            //这里就是 COUNT(*) 的结果，我们将其转换为 int
            object result = command.ExecuteScalar();//查询密码值
            if (result != null && result != DBNull.Value)
            {
                if(result==null)
                {
                    return -3;//意外错误，查询不到密码值
                }
                ans = Convert.ToString(result);
            }
        }
        if(ans!=password)
        {
            return -2;//密码错误
        }
        check = "SELECT purview FROM db_users WHERE name = '" + user_name + "'";
        using (OracleCommand command = new OracleCommand(check, connection))
        {
            //ExecuteScalar 返回结果集的第一行第一列的值
            //这里就是 COUNT(*) 的结果，我们将其转换为 int
            object result = command.ExecuteScalar();//查询密码值
            if (result != null && result != DBNull.Value)
            {
                count = Convert.ToInt32(result);
            }
        }
        return count;
    }
    public static int New_user(OracleConnection connection)//新建用户
    {
        Console.WriteLine("请输入用户参数：");
        string data = Console.ReadLine();//获取当前参数
        if (data == null)
        {
            return -1;//-1表示参数不正确
        }
        string action = "insert into db_users(";//action是要执行的sql语言
        string[] option = new string[7];
        int cnt = 0;
        int[] flag = { 0, 0, 0, 0, 0, 0, 0 };

        for (int i = 0; i < data.Length; i++)//进行参数划分
        {
            if (data[i] == ',')
            {
                if (basic_calculate.Check_null(option[cnt]) == 0)//如果当前项不为NULL
                {
                    //Console.WriteLine(option[cnt]);
                    flag[cnt] = 1;//当前位有效
                }
                cnt++;
                if (cnt > 6)//超限防溢出
                {
                    return -1;//表示参数输入错误
                }
                continue;
            }
            option[cnt] = option[cnt] + data[i];
        }
        if (basic_calculate.Check_null(option[cnt]) == 0)//对最后一项进行额外的检测
        {
            //Console.WriteLine(option[cnt]);
            flag[cnt] = 1;//当前位有效
        }
        if (cnt != 6)//防止参数数量不正确
        {
            return -1;//表示参数输入错误
        }
        if (flag[0] == 0 || flag[1]== 0 || flag[2] == 0 || flag[3] == 0 || flag[5] == 0 || flag[6] == 0 || flag[4] == 0)//如果not null位为null
        {
            return -1;//表示参数输入错误
        }
        
        string check = "SELECT COUNT(*) FROM db_users WHERE name = '" + option[0] + "'";
        int count = 0;
        Console.WriteLine(check);
        using (OracleCommand command = new OracleCommand(check, connection))
        {
            //ExecuteScalar 返回结果集的第一行第一列的值
            //这里就是 COUNT(*) 的结果，我们将其转换为 int
            object result = command.ExecuteScalar();
            if (result != null && result != DBNull.Value)
            {
                count = Convert.ToInt32(result);
            }
        }
        Console.WriteLine(count);
        if (count != 0)
        {
            return -2;//-2表示当前用户名已经注册
        }
        action = action + "name";
        if (flag[1] == 1)//密码
        {
            action = action + ",password";
        }
        if (flag[2] == 1)//性别
        {
            action = action + ",sex";
        }
        if (flag[3] == 1)//生日
        {
            action = action + ",birthday";
        }
        if (flag[4] == 1)//邮箱地址
        {
            action = action + ",E_mail_address";
        }
        if (flag[5] == 1)//注册时间
        {
            action = action + ",admission_time";
        }
        if (flag[6] == 1)//权限
        {
            action = action + ",purview";
        }
        action = action + ")\n";
        action = action + "values(";
        action = action + "'" + option[0] + "'";
        for (int i = 1; i < 7; i++)
        {
            if (flag[i] != 0)
            {
                action = action + ",'" + option[i] + "'";
            }
        }
        action = action + ")\n";
        Console.WriteLine(action);
        using (OracleCommand command = new OracleCommand(action, connection))
        {
            // 执行 INSERT 语句
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine($"INSERT 操作成功执行，影响了 {rowsAffected} 行。");
        }
        return 0;
    }
    public static int Change_purview(OracleConnection connection)//修改用户权限
    {
        Console.WriteLine("请输入修改参数：");
        //参数类型为名字+","+权限等级，例：ztz11,1
        string data = Console.ReadLine();//获取当前参数
        if (data == null)
        {
            return -1;//-1表示参数不正确
        }
        string action = "update db_users\n";//action是要执行的sql语言
        action = action + "set purview = ";
        string[] option = new string[2];
        int cnt = 0;
        int[] flag = { 0, 0 };

        for (int i = 0; i < data.Length; i++)//进行参数划分
        {
            if (data[i] == ',')
            {
                if (basic_calculate.Check_null(option[cnt]) == 0)//如果当前项不为NULL
                {
                    //Console.WriteLine(option[cnt]);
                    flag[cnt] = 1;//当前位有效
                }
                cnt++;
                if (cnt > 1)//超限防溢出
                {
                    return -1;//表示参数输入错误
                }
                continue;
            }
            option[cnt] = option[cnt] + data[i];
        }
        if (basic_calculate.Check_null(option[cnt]) == 0)//对最后一项进行额外的检测
        {
            //Console.WriteLine(option[cnt]);
            flag[cnt] = 1;//当前位有效
        }
        if(cnt!=1)
        {
            return -1;
        }
        int val;
        try
        {
            val = Convert.ToInt32(option[1]);//尝试转化权限
        }
        catch (Exception ex)//如果存在无法转换的异常
        {
            return -1;//返回参数错误
        }
        if(val<0||val>2)
        {
            return -1;//还是，参数不对
        }
        string check = "select count(*)\n"+"from db_users\n"+"where name = '";
        check = check + option[0]+"'";
        //Console.WriteLine(check);
        int count=0;
        using (OracleCommand command = new OracleCommand(check, connection))
        {
            //ExecuteScalar 返回结果集的第一行第一列的值
            //这里就是 COUNT(*) 的结果，我们将其转换为 int
            object result = command.ExecuteScalar();
            if (result != null && result != DBNull.Value)
            {
                count = Convert.ToInt32(result);
            }
            if(count <= 0)
            {
                return -2;//修改不存在的用户
            }
        }
        action = action + option[1];
        action = action + "\n";
        action = action + "where name = '";
        action = action + option[0];
        action = action + "'";
        //Console.WriteLine(action);
        using (OracleCommand command = new OracleCommand(action, connection))
        {
            // 执行 update 语句
            int rowsAffected = command.ExecuteNonQuery();
            Console.WriteLine($"update 操作成功执行，影响了 {rowsAffected} 行。");
        }
        return 0;
    }
}


namespace main
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. 配置连接字符串（请根据实际情况修改）
            string connectionString = "User Id=system;" +
                                      "Password=Tongji123;" +
                                      "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=47.121.177.100)(PORT=1521))" +
                                      "(CONNECT_DATA=(SERVICE_NAME=XEPDB1)))";
            // 2. 启动数据库链接
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("成功连接到 Oracle 数据库");
                }
                catch (Exception ex)
                {
                    // 捕获链接意外错误
                    Console.WriteLine($"链接错误: {ex.Message}");
                    return;
                }
                while (true)//开始监听
                {
                    try
                    {
                        Console.WriteLine("请输入操作");
                        string action = Console.ReadLine();
                        if (action == "close")//关闭程序
                        {
                            if (connection.State == System.Data.ConnectionState.Open)
                            {
                                connection.Close();
                                Console.WriteLine("数据库连接已关闭");
                            }
                            break;
                        }
                        if (action == "insert_normal")//基本信息表的修改
                        {
                            int val = db_insert.Insert_normal(connection);
                            if (val == -1)
                            {
                                Console.WriteLine("参数格式错误，操作无效！");
                            }
                            if (val == -2)
                            {
                                Console.WriteLine("当前插入值已在表中，请勿重复插入！");
                            }
                        }
                        if (action == "login")//登录操作
                        {
                            Console.WriteLine("请输入用户名：");
                            string user_name = Console.ReadLine();
                            Console.WriteLine("请输入密码：");
                            string password = Console.ReadLine();
                            if (user_name == null || password == null)
                            {
                                Console.WriteLine("参数错误！请重新输入");
                                continue;
                            }
                            int val = users.Login(user_name, password, connection);
                            if (val == -1)
                            {
                                Console.WriteLine("当前用户未注册！");
                                continue;
                            }
                            if (val == -2)
                            {
                                Console.WriteLine("密码错误！");
                                continue;
                            }
                            if (val == -3)
                            {
                                Console.WriteLine("意外错误！请联系管理员");
                                continue;
                            }
                            Console.Write("登陆成功！当前权限为：");
                            Console.WriteLine(val);
                        }
                        if (action == "new_user")
                        {
                            int val = users.New_user(connection);
                            if (val == -1)
                            {
                                Console.WriteLine("参数格式错误，操作无效！");
                                continue;
                            }
                            if (val == -2)
                            {
                                Console.WriteLine("当前用户名已经被注册！");
                                continue;
                            }
                        }
                        if(action== "change_purview")
                        {
                            int val=users.Change_purview(connection);
                            if (val == -1)
                            {
                                Console.WriteLine("参数错误！");
                                continue;
                            }
                            if (val == -2)
                            {
                                Console.WriteLine("你修改权限的用户不存在！");
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("发生操作：" + ex.Message);
                    }
                }
            }
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
}