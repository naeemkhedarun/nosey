package reducer

import org.apache.hadoop.io.{IntWritable, DoubleWritable, Text, MapWritable}

class PerformanceCounter(val statName: String,
                         val machineName: String,
                         val date: String,
                         val average: Double,
                         val max: Double,
                         val min: Double,
                         val count: Int = 1) extends Serializable {

  def merge(other: PerformanceCounter) =
    new PerformanceCounter(
      statName,
      machineName,
      date,
      (other.average + average) / 2,
      scala.math.max(other.max, max),
      scala.math.min(other.min, min),
      other.count + count)

  override def toString = "[%s][%s][%s] Average: %s, Max: %s, Min: %s, Count: %s".format(
    machineName, statName, date, average, max, min, count
  )

  def toMapWritable() = {
    val m = new MapWritable
      m.put(new Text("StatName"), new Text(statName))
      m.put(new Text("MachineName"), new Text(machineName))
      m.put(new Text("Date"), new Text(date))
      m.put(new Text("Average"), new DoubleWritable(average))
      m.put(new Text("Min"), new DoubleWritable(min))
      m.put(new Text("Max"), new DoubleWritable(max))
      m.put(new Text("Count"), new IntWritable(count))
    m
//    Map("StatName" -> statName, "MachineName" -> machineName, "Date" -> date, "Average" -> average, "Min" -> min, "Max" -> max, "Count" -> count)
  }

}
