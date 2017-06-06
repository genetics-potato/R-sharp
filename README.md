## R# language design

###### Code comments

```R
## This is code comments, it just only allow single line comments.
```

###### Variable

Variable in ``R#`` should be declared by ``var`` keyword, and its value assign is force using ``<-`` operator:

```R
var s <- "12345";
var x <- {1, 2, 3, 4, 5};
var m <- {
   {1, 2, 3},
   {4, 5, 6},
   {7, 8, 9}
};
```

Delcare a vector or matrix will no longer required of the ``c(...)`` function or ``matrix`` function. Almost keeps the same as the VisualBasic language it does:

```vbnet
Dim s = "12345"
Dim x = {1, 2, 3, 4, 5}
Dim m = {
   {1, 2, 3},
   {4, 5, 6},
   {7, 8, 9}
}
```

###### Types

``R#`` language have sevral primitive type:

+ ``integer``, System.Int64 vector
+ ``double``, System.Double vector
+ ``uinteger``, System.UInt64 vector
+ ``string``, System.String vector
+ ``char``, System.Char vector
+ ``boolean``, System.Boolean vector

And you can declare the user type by using ``list()`` function, example like:

```R
var obj <- list();

# using with for object property initialize
var obj <- list() with {
    $a <- 123;
	$b <- "+++";
}
```

generally, the parameter in a function is a generic type, so that a function definition like:

```R
test <- function(x) {
}
```

can accept any type you have input. but you can using the ``param as <type>`` for limit the type as a specific type:

```R
test.integer <- function(x as integer) {
    # the type constraint means the parameter only allow the integer vector type
	# if the parameter is a string vector, then the interpreter will throw exceptions.
}
```

###### Get/Set value

Get/Set property value keeps the same as the R language: 

```R
var names <- dataframe[, "name"];
dataframe[, "name"] <- new.names;
```

###### String

Add new string contact and string interploate feature for ``R#``:

```R
var name     <- first.name & " " & last.name;
# or
var my.name  <- "{first.name} {last.name}"; 
# sprintf function is still avaliable
var his.name <- sprintf("%s %s", first.name, last.name); 
```

###### Logical operators

+ and, andalso
+ or, orelse
+ not

```R
if (x <= 10 andalso y != 99) {
    # ......
} else if(not z is null) {
    # ......
}
```

###### Operator binding

Allows you bind operator on your custom type:

```R
# binding operator only allows in the with closure in the object declare statement
var me <- list() with {
   %+% <- function($, other) {
   }
   %is% <- function($, other) {
   }
}

# and then using the operator

var new.me    <- me + other;
var predicate <- me is other;

if (not me is him) {
    # ......
}
```

Allows user operator

|Operator |Description          |
|---------|---------------------|
|``+``    | add                 |
|``-``    | substract           |
|``*``    | multiply            |
|``/``    | devide              |
|``\``    | integer devide      |
|``%``    | mod                 |
|``^``    | power               |
|``is``   | object equals       |
|``like`` | object similarity   |
|``in``   | collection set      |
|``which``| index list for true |

###### IN operator

```R
# in list
var booleans <- name in names(obj);
# in range
var booleans <- x in [min, max];
```

###### combine with ``Which`` operator 

```R
var x <- {1, 2, 3, 4, 5};
var indices.true <- which x in [min, max];
```

###### Simple external calls

Makes more easier for calls external command from CLI, apply a ``@`` operator on a string vector will makes an external system calls:

```R
var [exitCode, stdout] <- @"/bin/GCModeller/localblast /blastp /query \"{query.fasta}\" /subject \"{COG_myva}\" /out \"{COG_myva.csv}\"";

# or makes it more clear to read
var CLI <- "/bin/GCModeller/localblast /blastp /query \"{query.fasta}\" /subject \"{COG_myva}\" /out \"{COG_myva.csv}\"";
var [exitCode, stdout] <- @CLI;
```

###### Using tuple

Tuple enable the R function returns multiple value at once:

```R
tuple.test <- function(a as integer, b as integer) {
    return [a, b, a^b];
}

var [a, b, c] <- tuple.test(3, 2);

if (a == 3) {
    c = c + a + b;
}
```

###### R object to tuple

You can naturally convert the object as tuple value. The member in the tuple their name should matched the names in an object, so that you can doing something like this example in ``R#``:

```R
var obj <- list() with {
    $a <- 333;
    $b <- 999;
}
var [a, b] <- obj;
```

But, wait, if the property in an object is not a valid identifier name in ``R#``? Don't worried, you can using alias:

```R
var obj <- list() with {
    $"112233+5" <- 999;
    $x <- 1;
}
var [a as "112233+5", b as "x"] <- obj;
```

The tuple feature is espacially useful in operates the dataframe:

```R
var d <- data.frame(
    a = {1,2,3},
    b = {"a","g","y"},
    t = {TRUE, TRUE, FALSE});

for([a,b,c] in d) {
    println("%s = %s ? (%s)", a,b,c);
}

var [a, b, booleans as "t"] <- d;
```

If the tuple is applied on a for loop, then it means convert each row in dataframe as tuple, or just applied the tuple on the var declaring, then it means converts the columns in dataframe as the tuple, so that the variable in tuple is a vector with nrows of the dataframe.