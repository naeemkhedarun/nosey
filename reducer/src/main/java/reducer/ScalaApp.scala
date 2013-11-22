package reducer

import java.util
import org.apache.spark.SparkContext
import org.apache.hadoop.io.{MapWritable, NullWritable}
import org.elasticsearch.hadoop.mr.{ESOutputFormat, ESInputFormat}
import org.apache.hadoop.mapred.JobConf
import org.elasticsearch.hadoop.util.WritableUtils
import scala.collection.Map
import org.apache.spark.rdd.PairRDDFunctions
import java.io.IOException

object App {
    def main(args: Array[String]) {

      val dateFormat = new java.text.SimpleDateFormat("dd/MM/yyyy HH:mm")
      val toDateFormat = new java.text.SimpleDateFormat("dd/MM/yyyy HH:mm")
      val sc = new SparkContext("local", "My App", System.getenv("SPARK_HOME"), Seq(System.getenv("SPARK_EXAMPLES_JAR")), Map.empty, Map.empty)

      sc.hadoopConfiguration.set("es.resource", "counter/machine/_search?size=500000")
      val conf = new JobConf(sc.hadoopConfiguration)
      val counters = sc.newAPIHadoopRDD[NullWritable, MapWritable, ESInputFormat[NullWritable, MapWritable]](conf, classOf[ESInputFormat[NullWritable, MapWritable]], classOf[NullWritable], classOf[MapWritable])

      val mappedCounters = counters
        .map({
          case (nullKey, writableValue) => {
            val sample = WritableUtils.fromWritable(writableValue).asInstanceOf[util.LinkedHashMap[Object, Object]]

            val date: String = toDateFormat.format(dateFormat.parse(sample.get("Date").asInstanceOf[String]))
            val value: Double = sample.get("Value").asInstanceOf[Double]
            val statName: String = sample.get("StatName").asInstanceOf[String]
            val machineName: String = sample.get("MachineName").asInstanceOf[String]

            new PerformanceCounter(statName, machineName, date, value, value, value, 1 )
          }
        })
        .groupBy((counter: PerformanceCounter) => counter.machineName)
        .map((tuple: (String, Seq[PerformanceCounter])) => tuple._2.reduce((counterX: PerformanceCounter, counterY: PerformanceCounter) => counterX.merge(counterY)))
              .map((counter: PerformanceCounter) => counter.toMapWritable())

      val output = mappedCounters.map { v => (NullWritable.get.asInstanceOf[Object], v.asInstanceOf[Object]) }

      val rdd = new PairRDDFunctions(output)

      sc.hadoopConfiguration.set("es.resource", "counter/machine_sec")
      val outputConf = new JobConf(sc.hadoopConfiguration)

      try{
        rdd.saveAsHadoopFile("-", classOf[Object], classOf[Object], classOf[ESOutputFormat], outputConf, None)
      }
      catch{
        case ex: IOException => {
          println("Not sure why this is happening...")
        }
      }
    }
  }
